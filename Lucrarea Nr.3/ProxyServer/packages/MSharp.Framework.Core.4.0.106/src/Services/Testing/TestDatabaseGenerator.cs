namespace MSharp.Framework.Services
{
    using MSharp.Framework.Data;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public abstract class TestDatabaseGenerator
    {
        const string TEMP_DATABASES_LOCATION_KEY = "Temp.Databases.Location";
        const string MSHARP_META_DIRECTORY_KEY = "M#.Meta.Location";

        public static Action CreateReferenceData;
        protected static object SyncLock = new object();
        protected static object ProcessSyncLock = new object();

        protected string ConnectionString;

        public static TestDatabaseGenerator Current = new SqlTestDatabaseGenerator();

        protected string TempDatabaseName, ReferenceDatabaseName;

        protected DirectoryInfo TempBackupsRoot, ProjectTempRoot, MSharpMetaDirectory, CurrentHashDirectory;

        protected bool IsTempDatabaseOptional, MustRenew;

        public virtual TestDatabaseGenerator Initialize(bool isTempDatabaseOptional, bool mustRenew)
        {
            ConnectionString = DataAccessor.Current.GetCurrentConnectionString();

            IsTempDatabaseOptional = isTempDatabaseOptional;

            MustRenew = mustRenew;

            return this;
        }

        FileInfo[] GetCreateDbFiles()
        {
            if (MSharpMetaDirectory == null)
                LoadMSharpMetaDirectory();

            var msharpScriptsDirectory = MSharpMetaDirectory.GetSubDirectory("Current", onlyWhenExists: true) ?? MSharpMetaDirectory;
            var manualScriptsDirectory = MSharpMetaDirectory.GetSubDirectory("Manual", onlyWhenExists: true) ?? MSharpMetaDirectory;

            var tableScripts = msharpScriptsDirectory.GetSubDirectory("Tables").GetFilesOrEmpty("*.sql");

            var potentialSources = new List<FileInfo>();

            // Create tables:
            potentialSources.Add(msharpScriptsDirectory.GetFile("@Create.Database.sql"));
            potentialSources.AddRange(tableScripts.Except(x => x.Name.ToLower().EndsWithAny(".fk.sql", ".data.sql")));

            // Insert data:
            potentialSources.Add(msharpScriptsDirectory.GetFile("@Create.Database.Data.sql"));
            potentialSources.AddRange(tableScripts.Where(x => x.Name.ToLower().EndsWith(".data.sql")));
            potentialSources.AddRange(msharpScriptsDirectory.GetSubDirectory("Data").GetFilesOrEmpty("*.sql"));

            potentialSources.Add(manualScriptsDirectory.GetFile("Customize.Database.sql"));

            // Add foreign keys
            potentialSources.Add(msharpScriptsDirectory.GetFile("@Create.Database.ForeignKeys.sql"));
            potentialSources.AddRange(tableScripts.Where(x => x.Name.ToLower().EndsWith(".fk.sql")));

            var sources = potentialSources.Where(f => f.Exists()).ToList();

            if (sources.None())
                throw new Exception("No SQL creation script file was found. I checked:\r\n" + potentialSources.ToLinesString());

            return sources.ToArray();
        }

        Dictionary<FileInfo, string> GetExecutableCreateDbScripts()
        {
            var sources = GetCreateDbFiles();

            var result = new Dictionary<FileInfo, string>();

            foreach (var file in sources)
            {
                var script = file.ReadAllText();

                script = script.ToLines().Select((line, index) =>
                {
                    if (index < 10)
                    {
                        return line
                        .Replace("#DATABASE.NAME#", ReferenceDatabaseName)
                        .Replace("#STORAGE.PATH#", CurrentHashDirectory.FullName)
                        .Replace("#SERVER#", DefaultServerName)
                        .Replace("#UID#", DefaultUserID)
                        .Replace("#PASSWORD#", DefaultPassword);
                    }

                    return line;
                }).ToLinesString();

                if (file.Name.Lacks("Create.Database.sql", caseSensitive: false))
                    script = GetDataBaseNameToUse() + script;

                result.Add(file, script);
            }

            return result;
        }

        public string GetCurrentDatabaseCreationHash()
        {
            var createScript = GetCreateDbFiles().Select(x => x.ReadAllText()).ToLinesString();

            var scriptsFolder = AppDomain.CurrentDomain.BaseDirectory.AsDirectory().Parent
                .GetSubDirectory("Domain\\[DEV-SCRIPTS]", onlyWhenExists: false);

            if (scriptsFolder.Exists())
                createScript += scriptsFolder
                    .GetFiles("*.*")
                    .Select(x => x.ReadAllText())
                    .ToLinesString();

            return createScript.ToSimplifiedSHA1Hash();
        }

        void EnsurePermissions()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;

            var error = "\r\n\r\nRecommended action: If using IIS, update the Application Pool (Advanced Settings) and set Identity to LocalSystem.";

            if (identity.IsEmpty())
            {
                error = "Current IIS process model Identity not found!" + error;
                throw new Exception(error);
            }
            else
                error = "Current IIS process model Identity: " + identity + error;

            if (identity.ContainsAny(new[] { "IIS APPPOOL", "LOCAL SERVICE", "NETWORK SERVICE" }))
            {
                error = "In TDD mode full system access is needed in order to create temporary database files." + error;
                throw new Exception(error);
            }
        }

        void LoadTempDatabaseLocation()
        {
            var specifiedLocation = Config.Get(TEMP_DATABASES_LOCATION_KEY);

            if (specifiedLocation.IsEmpty())
            {
                throw new Exception("You must specify a valid path for AppSetting of '{0}'.".FormatWith(TEMP_DATABASES_LOCATION_KEY));
            }

            if (!specifiedLocation.AsDirectory().Exists())
            {
                // Try to build once:
                try
                {
                    Directory.CreateDirectory(specifiedLocation);
                }
                catch
                {
                    throw new Exception("Could not create the folder '{0}'. Ensure it exists and is accessible. Otherwise specify a different location in AppSetting of '{1}'."
                        .FormatWith(specifiedLocation, TEMP_DATABASES_LOCATION_KEY));
                }
            }

            TempBackupsRoot = specifiedLocation.AsDirectory();
            ProjectTempRoot = TempBackupsRoot.GetOrCreateSubDirectory(TempDatabaseName);
        }

        void LoadMSharpMetaDirectory()
        {
            // Explicitly specified?
            var specified = Config.Get(MSHARP_META_DIRECTORY_KEY);

            if (specified.HasValue())
            {
                if (!specified.AsDirectory().Exists())
                {
                    var error = "The path '{0}' does not exist. Specify the correct path of @M# folder via AppSetting of '{1}'.".FormatWith(specified, MSHARP_META_DIRECTORY_KEY);
                    error += "Maybe the current context user ({0}) does not have access to this network path?".FormatWith(Environment.UserName);
                    throw new Exception(error);
                }
                else
                {
                    // Already exists:
                    MSharpMetaDirectory = specified.AsDirectory();
                    return;
                }
            }
            else
            {
                // Not explicitly specified. Take a guess:

                var options = new[] { "../@M#", "../../@M#", "../../../@M#" };
                foreach (var option in options)
                {
                    var folder = AppDomain.CurrentDomain.GetPath(option).AsDirectory();

                    if (folder.Exists())
                    {
                        MSharpMetaDirectory = folder;
                        return;
                    }
                }

                throw new Exception("Please specify the path of @M# folder via AppSetting of '{0}'.".FormatWith(MSHARP_META_DIRECTORY_KEY));
            }
        }

        bool IsExplicitlyTempDatabase() => TempDatabaseName.ToLower().EndsWith(".temp");

        public bool Process()
        {
            if (ConnectionString.IsEmpty()) return false;

            TempDatabaseName = DefaultDatabaseName.Or("").TrimStart("[").TrimEnd("]");

            if (TempDatabaseName.IsEmpty())
            {
                return false;
            }
            else if (!TempDatabaseName.ToLower().EndsWith(".temp") && IsTempDatabaseOptional)
            {
                return false;
            }

            EnsurePermissions();

            LoadTempDatabaseLocation();

            LoadMSharpMetaDirectory();

            if (!IsTempDatabaseOptional)
            {
                if (!IsExplicitlyTempDatabase())
                {
                    throw new Exception("For unit tests project the database name must end in '.Temp'.");
                }
            }

            if (!IsExplicitlyTempDatabase())
            {
                // Not Temp mode:
                return false;
            }

            return DoProcess();
        }

        public void CleanUp() => DataAccessor.Current.DeleteDatabase(TempDatabaseName);

        protected void CreateDatabaseFromScripts()
        {
            DataAccessor.Current.DeleteDatabase(ReferenceDatabaseName);

            using (var connection = DataAccessor.Current.CreateConnection(DataAccessor.Current.GetMasterConnectionString()))
            {
                foreach (var file in GetExecutableCreateDbScripts())
                {
                    var lines = new Regex(@"^\s*GO\s*$", RegexOptions.Multiline).Split(file.Value);

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;

                        foreach (var line in lines.Trim())
                        {
                            cmd.CommandText = line;

                            try { cmd.ExecuteNonQuery(); }
                            catch (Exception ex)
                            { throw new Exception("Could not execute sql file '" + file.Key.FullName + "' becuase '" + ex.Message + "'", ex); }
                        }
                    }
                }
            }

            CreateReferenceData?.Invoke();
        }

        protected void RefreshTempDataWorld()
        {
            CloneReferenceDatabaseToTemp();

            SqlConnection.ClearAllPools();

            CopyFiles();

            DataAccessor.Current.TestDatabaseConnection(DefaultDatabaseName);
        }

        protected string GetSnapshotFileName(FileInfo file) => file.Name.Split('.').First() + ".Temp";

        void CopyFiles()
        {
            var copyActions = new List<Action>();

            foreach (
                var key in
                    new[]
                        {
                            Tuple.Create("Test.Files.Origin", "UploadFolder"),
                            Tuple.Create("Test.Files.Origin.Secure", "UploadFolder.Secure")
                        })
            {
                var source = Config.Get(key.Item1);
                if (source.IsEmpty()) continue;
                else source = AppDomain.CurrentDomain.GetPath(source);
                if (!Directory.Exists(source) || source.AsDirectory().GetDirectories().None())
                {
                    // No files to copy
                    continue;
                }

                var destination = Config.Get(key.Item2);
                if (destination.IsEmpty())
                    throw new Exception("Destination directory not configured in App.Config for key: " + key.Item2);
                else destination = AppDomain.CurrentDomain.GetPath(destination);

                if (!Directory.Exists(destination))
                {
                    if (new DirectoryInfo(source).IsEmpty()) continue;

                    Directory.CreateDirectory(destination);
                }

                new DirectoryInfo(destination).Clear();

                copyActions.Add(delegate { new DirectoryInfo(source).CopyTo(destination, overwrite: true); });
            }

            copyActions.Do(a => a?.Invoke());
        }

        public abstract string DefaultDatabaseName { get; }
        public abstract string DefaultServerName { get; }
        public abstract string DefaultUserID { get; }
        public abstract string DefaultPassword { get; }

        public abstract void SnapshotDatabase(DirectoryInfo snapshotsDirectory, bool isInShareSnapshotMode);
        public abstract void RestoreDatabase(DirectoryInfo snapshotsDirectory, bool isInShareSnapshotMode);
        protected abstract bool DoProcess();
        protected abstract void CloneReferenceDatabaseToTemp();
        protected abstract string GetDataBaseNameToUse();

    }
}