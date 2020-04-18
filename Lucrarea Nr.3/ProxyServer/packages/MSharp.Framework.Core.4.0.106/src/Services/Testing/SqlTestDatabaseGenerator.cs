namespace MSharp.Framework.Services
{
    using MSharp.Framework;
    using MSharp.Framework.Data;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    public class SqlTestDatabaseGenerator : TestDatabaseGenerator
    {
        FileInfo ReferenceMDFFile, ReferenceLDFFile;
        SqlConnectionStringBuilder DefaultConnectionSting => new SqlConnectionStringBuilder(DataAccessor.GetCurrentConnectionString());

        public override string DefaultDatabaseName
        {
            get
            {
                try
                {
                    return DefaultConnectionSting.InitialCatalog;
                }
                catch
                {
                    return null;
                }
            }
        }

        public override string DefaultServerName => DefaultConnectionSting.DataSource;

        public override string DefaultUserID => DefaultConnectionSting.UserID;

        public override string DefaultPassword => DefaultConnectionSting.Password;

        protected override void CloneReferenceDatabaseToTemp()
        {
            // Make sure if it exists in database already, it's deleted first.
            DataAccessor.Current.DeleteDatabase(TempDatabaseName);

            var directory = ProjectTempRoot.GetOrCreateSubDirectory("Current");

            var newMDFPath = directory.GetFile(TempDatabaseName + ".mdf");
            var newLDFPath = directory.GetFile(TempDatabaseName + "_log.ldf");

            try
            {
                ReferenceMDFFile.CopyTo(newMDFPath);
                ReferenceLDFFile.CopyTo(newLDFPath);
            }
            catch (IOException ex)
            {
                if (ex.InnerException != null && ex.InnerException is UnauthorizedAccessException)
                    throw new Exception("Consider setting the IIS Application Pool identity to LocalSystem.", ex);

                throw;
            }

            var script = "CREATE DATABASE [{0}] ON (FILENAME = '{1}'), (FILENAME = '{2}') FOR ATTACH"
                .FormatWith(TempDatabaseName, newMDFPath.FullName, newLDFPath.FullName);

            using (new DatabaseContext(DataAccessor.Current.GetMasterConnectionString()))
            {
                try
                {
                    DataAccessor.Current.ExecuteNonQuery(script);
                }
                catch (SqlException ex)
                {
                    throw new Exception("Could not attach the database from file " + newMDFPath.FullName + "." + Environment.NewLine +
                    "Hint: Ensure SQL instance service has access to the folder. E.g. 'Local Service' may not have access to '{0}'" +
                    newMDFPath.Directory.FullName, ex);
                }
            }
        }

        protected override bool DoProcess()
        {
            var hash = GetCurrentDatabaseCreationHash().Replace("/", "-").Replace("\\", "-");

            lock (SyncLock)
            {
                ReferenceDatabaseName = TempDatabaseName;

                CurrentHashDirectory = ProjectTempRoot.GetOrCreateSubDirectory(hash);
                ReferenceMDFFile = CurrentHashDirectory.GetFile(ReferenceDatabaseName + ".mdf");
                ReferenceLDFFile = CurrentHashDirectory.GetFile(ReferenceDatabaseName + "_log.ldf");

                lock (ProcessSyncLock)
                {
                    var createdNewReference = CreateReferenceDatabase();

                    var tempDatabaseDoesntExist = !DataAccessor.Current.DatabaseExists(TempDatabaseName);

                    if (MustRenew || createdNewReference || tempDatabaseDoesntExist)
                    {
                        RefreshTempDataWorld();
                    }
                }

                return true;
            }
        }

        protected override string GetDataBaseNameToUse() => "USE [" + ReferenceDatabaseName + "];\r\nGO\r\n";

        private bool CreateReferenceDatabase()
        {
            if (ReferenceMDFFile.Exists() && ReferenceLDFFile.Exists())
            {
                return false;
            }

            var error = false;

            // create database + data
            try
            {
                var start = LocalTime.Now;
                CreateDatabaseFromScripts();
            }
            catch
            {
                error = true;
                throw;
            }
            finally
            {
                // Detach it
                DataAccessor.Current.DetachDatabase(ReferenceDatabaseName);

                if (error)
                {
                    ReferenceMDFFile.Delete(harshly: true);
                    ReferenceLDFFile.Delete(harshly: true);
                }
            }

            return true;
        }

        #region DB Snapshot

        static Mutex SnapshotRestoreLock;

        public override void SnapshotDatabase(DirectoryInfo snapshotsDirectory, bool isInShareSnapshotMode)
        {
            SqlConnection.ClearAllPools();

            FileInfo[] files;
            using (var connection = new SqlConnection(DataAccessor.Current.GetMasterConnectionString()))
            {
                connection.Open();
                files = GetPhysicalFiles(connection);
            }

            DataAccessor.Current.TakeDatabaseOffline(DefaultDatabaseName);

            files.Do(f =>
            {
                if (isInShareSnapshotMode)
                {
                    f.CopyTo(Path.Combine(snapshotsDirectory.FullName, GetSnapshotFileName(f) + f.Extension));

                    // keep the snashptname of the database in a .origin file
                    File.WriteAllText(snapshotsDirectory.GetFile(
                    GetSnapshotFileName(f) + f.Extension + ".origin").FullName,
                    f.FullName.Replace(DefaultDatabaseName, GetSnapshotFileName(f)));
                }
                else
                {
                    f.CopyTo(snapshotsDirectory);
                    // keep the original location of the database file in a .origin file
                    File.WriteAllText(snapshotsDirectory.GetFile(f.Name + ".origin").FullName, f.FullName);
                }
            });

            DataAccessor.Current.TakeDatabaseOnline(DefaultDatabaseName);
        }

        public override void RestoreDatabase(DirectoryInfo snapshotsDirectory, bool isInShareSnapshotMode)
        {
            SnapshotRestoreLock = new Mutex(false, "SnapshotRestore");
            var lockTaken = false;

            try
            {
                lockTaken = SnapshotRestoreLock.WaitOne();
                var restoreTime = LocalTime.Now;

                var detachTime = LocalTime.Now;

                DataAccessor.Current.DetachDatabase(DefaultDatabaseName);

                Debug.WriteLine("Total time for detaching database: " + LocalTime.Now.Subtract(detachTime).Milliseconds);

                FileInfo mdfFile = null, ldfFile = null;

                var copyTime = LocalTime.Now;
                // copy each database file to its old place
                foreach (var originFile in snapshotsDirectory.GetFiles("*.origin"))
                {
                    originFile.IsReadOnly = true;

                    var destination = File.ReadAllText(originFile.FullName);
                    var source = originFile.FullName.TrimEnd(originFile.Extension).AsFile();

                    if (isInShareSnapshotMode)
                    {
                        destination = destination.Replace(GetSnapshotFileName(originFile), DefaultDatabaseName);
                    }

                    if (destination.ToLower().EndsWith(".mdf"))
                        mdfFile = destination.AsFile();

                    if (destination.ToLower().EndsWith(".ldf"))
                        ldfFile = destination.AsFile();

                    source.CopyTo(destination, overwrite: true);
                    // shall we backup the existing one and in case of any error restore it?
                }

                Debug.WriteLine("Total time for copying database: " + LocalTime.Now.Subtract(copyTime).Milliseconds);

                if (mdfFile == null)
                    throw new Exception("Cannot find any MDF file in snapshot directory " + snapshotsDirectory.FullName);

                if (ldfFile == null)
                    throw new Exception("Cannot find any LDF file in snapshot directory " + snapshotsDirectory.FullName);
                var attachTime = LocalTime.Now;

                (DataAccessor.Current as SqlDataAccessor).AttachDatabase(mdfFile, ldfFile, DefaultDatabaseName);

                Debug.WriteLine("Total time for attaching database: " + LocalTime.Now.Subtract(attachTime).Milliseconds);
                Database.Refresh();

                Debug.WriteLine("Total time for restoreing database: " + LocalTime.Now.Subtract(restoreTime).Milliseconds);
            }
            finally
            {
                if (lockTaken == true)
                {
                    SnapshotRestoreLock.ReleaseMutex();
                }
            }
        }

        private FileInfo[] GetPhysicalFiles(SqlConnection connection)
        {
            var files = new List<FileInfo>();

            using (var cmd = new SqlCommand(
                "USE Master; SELECT physical_name FROM sys.master_files where database_id = DB_ID('{0}')"
                .FormatWith(DefaultDatabaseName), connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    files.Add(Convert.ToString(reader[0]).AsFile());
            }

            if (files.Count == 0)
                throw new Exception("Cannot find physical file name for database: " + DefaultDatabaseName);

            return files.ToArray();
        }

        #endregion
    }
}