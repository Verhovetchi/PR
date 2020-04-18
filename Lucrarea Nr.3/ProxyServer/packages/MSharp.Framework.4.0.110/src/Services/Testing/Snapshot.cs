namespace MSharp.Framework.Services.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;

    class Snapshot
    {
        const string TEMP_DATABASES_LOCATION_KEY = "Temp.Databases.Location";
        const string URL_FILE_NAME = "url.txt";
        const string DATE_FILE_NAME = "date.txt";
        string SnapshotName;
        bool IsInShareSnapshotMode;
        DirectoryInfo SnapshotsDirectory;

        public Snapshot(string name, bool isSharedSNapshotMode)
        {
            IsInShareSnapshotMode = isSharedSNapshotMode;
            SnapshotName = CreateSnapshotName(name);
            SnapshotsDirectory = GetSnapshotsRoot(IsInShareSnapshotMode).GetSubDirectory(SnapshotName, onlyWhenExists: false);
        }

        public void Create(HttpContext context)
        {
            if (IsSnapshotsDisabled) return;

            SetupDirecory();
            TestDatabaseGenerator.Current.SnapshotDatabase(SnapshotsDirectory, IsInShareSnapshotMode);
            CreateSnapshotCookies(context);
            CopyUploadedFiles(CopyProcess.Backup);
            SaveDate();
            SaveUrl(context);
        }

        static bool IsSnapshotsDisabled => Config.Get<bool>("WebTestManager.DisableSnapshots");

        public bool Exists()
        {
            if (IsSnapshotsDisabled) return false;

            return SnapshotsDirectory.Exists();
        }

        public void Restore(HttpContext context)
        {
            if (!Exists())
                throw new DirectoryNotFoundException("Cannot find snapshot " + SnapshotName);

            var restoreDatabase = LocalTime.Now;
            TestDatabaseGenerator.Current.RestoreDatabase(SnapshotsDirectory, IsInShareSnapshotMode);

            Debug.WriteLine("Total time for restoring including mutex: " + LocalTime.Now.Subtract(restoreDatabase).Milliseconds);

            var restoreCookies = LocalTime.Now;
            RestoreCookies(context);
            Debug.WriteLine("Total time for restoring cookies: " + LocalTime.Now.Subtract(restoreCookies).Milliseconds);

            var restoreFiles = LocalTime.Now;
            CopyUploadedFiles(CopyProcess.Restore);
            Debug.WriteLine("Total time for restoring files: " + LocalTime.Now.Subtract(restoreFiles).Milliseconds);

            var restoreDate = LocalTime.Now;
            RestoreDate();
            Debug.WriteLine("Total time for restoring date: " + LocalTime.Now.Subtract(restoreDate).Milliseconds);

            var restoreUrl = LocalTime.Now;
            RestoreUrl(context);
            Debug.WriteLine("Total time for restoring url: " + LocalTime.Now.Subtract(restoreUrl).Milliseconds);
        }

        void CopyUploadedFiles(CopyProcess process)
        {
            var copyActions = new List<Action>();

            foreach (var key in new[] { "UploadFolder", "UploadFolder.Secure" })
            {
                var source = Config.Get(key);
                if (source.IsEmpty())
                {
                    Debug.WriteLine("Destination directory not configured in App.Config for key: " + key);
                    continue;
                }

                var folder = Config.Get(key);
                if (folder.ToCharArray()[0] == '/') folder = folder.Substring(1);

                if (process == CopyProcess.Restore)
                {
                    source = Path.Combine(SnapshotsDirectory.ToString(), folder);
                    if (!Directory.Exists(source)) continue;
                    copyActions.Add(delegate { new DirectoryInfo(source).CopyTo(AppDomain.CurrentDomain.GetPath(Config.Get(key)), overwrite: true); });
                }
                else if (process == CopyProcess.Backup)
                {
                    source = AppDomain.CurrentDomain.GetPath(source);
                    if (!Directory.Exists(source)) continue;
                    copyActions.Add(delegate { new DirectoryInfo(source).CopyTo(Path.Combine(SnapshotsDirectory.ToString(), folder), overwrite: true); });
                }
            }

            copyActions.AsParallel().Do(a => a?.Invoke());
        }

        void SaveDate()
        {
            if (LocalTime.IsRedefined)
            {
                File.WriteAllText(SnapshotsDirectory.GetFile(DATE_FILE_NAME).FullName, LocalTime.Now.ToString());
            }
        }

        void RestoreDate()
        {
            var dateFile = SnapshotsDirectory.GetFile(DATE_FILE_NAME);
            if (dateFile.Exists())
            {
                var dateTime = Convert.ToDateTime(dateFile.ReadAllText());
                LocalTime.RedefineNow(() => dateTime);
            }
        }

        public static void RemoveSnapshots()
        {
            var sharedSnapshots = GetSnapshotsRoot(isSharedSnapshotMode: true);
            if (sharedSnapshots.Exists)
            {
                DeleteDirectory(sharedSnapshots);
                sharedSnapshots.EnsureExists();
            }

            var normalSnapshots = GetSnapshotsRoot(isSharedSnapshotMode: false);
            if (normalSnapshots.Exists)
            {
                DeleteDirectory(normalSnapshots);
                normalSnapshots.EnsureExists();
            }

            HttpContext.Current.Response.Redirect("~/");
        }

        public static void RemoveSnapshot(string name)
        {
            var snapshotName = CreateSnapshotName(name);

            var normalSnapshotDirectory = Path.Combine(GetSnapshotsRoot(isSharedSnapshotMode: false).FullName, snapshotName).AsDirectory();
            if (normalSnapshotDirectory.Exists)
                DeleteDirectory(normalSnapshotDirectory);

            var shardSnapshotDirectory = Path.Combine(GetSnapshotsRoot(isSharedSnapshotMode: true).FullName, snapshotName).AsDirectory();
            if (shardSnapshotDirectory.Exists)
                DeleteDirectory(shardSnapshotDirectory);

            HttpContext.Current.Response.Redirect("~/");
        }

        public static void DeleteDirectory(DirectoryInfo targetDirectory)
        {
            var files = targetDirectory.GetFiles();
            var dirs = targetDirectory.GetDirectories();

            foreach (var file in files)
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            foreach (var dir in dirs)
                DeleteDirectory(dir);

            targetDirectory.Delete();
        }

        #region URL

        void SaveUrl(HttpContext context)
        {
            var url = HttpContext.Current.Request.Url.PathAndQuery;

            url = url.Substring(0, url.IndexOf("Web.Test.Command", StringComparison.OrdinalIgnoreCase) - 1);
            if (url.HasValue())
            {
                File.WriteAllText(SnapshotsDirectory.GetFile(URL_FILE_NAME).FullName, url);
                context.Response.Redirect(url);
            }
        }

        void RestoreUrl(HttpContext context)
        {
            var urlFile = SnapshotsDirectory.GetFile(URL_FILE_NAME);
            if (urlFile.Exists())
            {
                context.Response.Redirect(context.Request.Url.GetWebsiteRoot() + urlFile.ReadAllText().TrimStart("/"));
            }
        }
        #endregion

        #region Cookie
        void CreateSnapshotCookies(HttpContext context)
        {
            var json = JsonConvert.SerializeObject(
                    context.Request.GetCookies().Select(CookieStore.FromHttpCookie));

            GetCookiesFile().WriteAllText(json);
        }

        void RestoreCookies(HttpContext context)
        {
            var cookiesFile = GetCookiesFile();

            if (!cookiesFile.Exists()) return;

            var cookies = JsonConvert.DeserializeObject<CookieStore[]>(cookiesFile.ReadAllText());

            context.Response.Cookies.Clear();
            foreach (var cookie in cookies)
                context.Response.Cookies.Add(cookie.ToHttpCookie());
        }

        FileInfo GetCookiesFile() => SnapshotsDirectory.GetFile("cookies.json");

        class CookieStore
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public bool Secure { get; set; }
            public bool HttpOnly { get; set; }
            public string Domain { get; set; }
            public DateTime Expires { get; set; }
            public string Value { get; set; }

            public static CookieStore FromHttpCookie(HttpCookie cookie)
            {
                return new CookieStore
                {
                    Name = cookie.Name,
                    Path = cookie.Path,
                    Secure = cookie.Secure,
                    HttpOnly = cookie.HttpOnly,
                    Domain = cookie.Domain,
                    Expires = cookie.Expires,
                    Value = cookie.Value
                };
            }

            public HttpCookie ToHttpCookie()
            {
                return new HttpCookie(Name, Value)
                {
                    Path = Path,
                    Secure = Secure,
                    HttpOnly = HttpOnly,
                    Domain = Domain,
                    Expires = Expires
                };
            }
        }

        #endregion

        void SetupDirecory()
        {
            // make sure it is empty
            if (SnapshotsDirectory.Exists())
            {
                SnapshotsDirectory.Delete(recursive: true);
            }

            SnapshotsDirectory.Create();
        }

        /// <summary>
        /// Gets the list of current snapshots on disk.
        /// </summary>
        public static List<string> GetList(bool isSharedSnapshotMode)
        {
            if (!GetSnapshotsRoot(isSharedSnapshotMode).Exists()) return null;

            return GetSnapshotsRoot(isSharedSnapshotMode).GetDirectories().Select(f => f.Name.Substring(0, f.Name.LastIndexOf('_'))).ToList();
        }

        static DirectoryInfo GetSnapshotsRoot(bool isSharedSnapshotMode)
        {
            if (isSharedSnapshotMode)
            {
                return Path.Combine(Config.Get(TEMP_DATABASES_LOCATION_KEY), TestDatabaseGenerator.Current.DefaultDatabaseName.Split('.').First() + " SNAPSHOTS").AsDirectory();
            }
            else
            {
                return Path.Combine(Config.Get(TEMP_DATABASES_LOCATION_KEY), TestDatabaseGenerator.Current.DefaultDatabaseName, "SNAPSHOTS").AsDirectory();
            }
        }

        static string CreateSnapshotName(string name)
        {
            var schemaHash = TestDatabaseGenerator.Current.Initialize(false, false).GetCurrentDatabaseCreationHash();
            return "{0}_{1}".FormatWith(name, schemaHash).Except(Path.GetInvalidFileNameChars()).ToString("");
        }

        enum CopyProcess { Backup, Restore }
    }
}