namespace System
{
    using Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Threading.Tasks;

    partial class MSharpExtensions
    {
        static readonly Encoding DefaultEncoding = Encoding.UTF8; //Encoding.GetEncoding(1252);

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static byte[] ReadAllBytes(this FileInfo file)
        {
            return TryHard(file, delegate { return File.ReadAllBytes(file.FullName); }, "The system cannot read the file: {0}");
        }


        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static string ReadAllText(this FileInfo file) => ReadAllText(file, DefaultEncoding);

        public static string NameWithoutExtension(this FileInfo file) => Path.GetFileNameWithoutExtension(file.FullName);

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static string ReadAllText(this FileInfo file, Encoding encoding)
        {
            Func<string> readFile = () =>
            {
                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream, encoding))
                        return reader.ReadToEnd();
                }

                //    return File.ReadAllText(file.FullName, encoding); 
            };

            return TryHard(file, readFile, "The system cannot read the file: {0}");
        }

        /// <summary>
        /// Will set the Position to zero, and then copy all bytes to a memory stream's buffer.
        /// </summary>
        public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.Position = 0;
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Will try to delete a specified directory by first deleting its sub-folders and files.
        /// </summary>
        /// <param name="harshly">If set to true, then it will try multiple times, in case the file is temporarily locked.</param>
        public static void Delete(this FileInfo file, bool harshly)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!file.Exists()) return;

            if (!harshly)
            {
                file.Delete();
                return;
            }

            TryHard(file, file.Delete, "The system cannot delete the file, even after several attempts. Path: {0}");
        }

        /// <summary>
        /// Saves the specified content on this file.
        /// </summary>
        public static void WriteAllBytes(this FileInfo file, byte[] content)
        {
            if (!file.Directory.Exists())
                file.Directory.Create();

            TryHard(file, delegate { File.WriteAllBytes(file.FullName, content); }, "The system cannot write the specified content on the file: {0}");
        }

        /// <summary>
        /// Saves the specified content on this file using the Western European Windows Encoding 1252.
        /// </summary>
        public static void WriteAllText(this FileInfo file, string content) => WriteAllText(file, content, DefaultEncoding);

        /// <summary>
        /// Saves the specified content on this file. 
        /// Note: For backward compatibility, for UTF-8 encoding, it will always add the BOM signature.
        /// </summary>
        public static void WriteAllText(this FileInfo file, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;

            file.Directory.EnsureExists();

            if (encoding is UTF8Encoding) encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            for (var retry = 5; true; retry--)
            {
                try
                {
                    using (var writer = new StreamWriter(file.FullName, append: false, encoding: encoding))
                        writer.Write(content);

                    return;
                }
                catch (IOException)
                {
                    if (retry <= 0) throw;
                    // No logging is needed. Will just retry.
                    Threading.Thread.Sleep(100.Milliseconds());
                }
            }
        }

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static void AppendAllText(this FileInfo file, string content) => AppendAllText(file, content, DefaultEncoding);

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static void AppendLine(this FileInfo file, string content = null)
        {
            AppendAllText(file, content + Environment.NewLine, DefaultEncoding);
        }

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static void AppendAllText(this FileInfo file, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;

            file.Directory.EnsureExists();

            File.AppendAllText(file.FullName, content, encoding);
        }

        /// <summary>
        /// Copies this file onto the specified desination path.
        /// </summary>
        public static void CopyTo(this FileInfo file, FileInfo destinationPath, bool overwrite = true)
        {
            if (!overwrite && destinationPath.Exists()) return;

            var content = file.ReadAllBytes();
            destinationPath.WriteAllBytes(content);
        }

        /// <summary>
        /// Writes the specified content on this file, only when this file does not already have the same content.
        /// </summary>
        public static bool WriteWhenDifferent(this FileInfo file, string newContent, Encoding encoding)
        {
            if (file.Exists())
            {
                var oldContent = file.ReadAllText();
                if (newContent == oldContent) return false;
            }

            file.WriteAllText(newContent, encoding);
            return true;
        }

        /// <summary>
        /// Determines whether or not this directory exists.
        /// Note: The standard Exists property has a caching bug, so use this for accurate result.
        /// </summary>
        public static bool Exists(this DirectoryInfo folder)
        {
            if (folder == null) return false;
            return IO.Directory.Exists(folder.FullName);
        }

        /// <summary>
        /// Determines whether or not this file exists. 
        /// Note: The standard Exists property has a caching bug, so use this for accurate result.
        /// </summary>
        public static bool Exists(this FileInfo file)
        {
            if (file == null) return false;
            return File.Exists(file.FullName);
        }

        /// <summary>
        /// Compresses this data into Gzip.
        /// </summary>
        public static byte[] GZip(this byte[] data)
        {
            using (var outFile = new MemoryStream())
            {
                using (var inFile = new MemoryStream(data))
                using (var Compress = new GZipStream(outFile, CompressionMode.Compress))
                    inFile.CopyTo(Compress);

                return outFile.ToArray();
            }
        }

        /// <summary>
        /// Compresses this string into Gzip. By default it will use UTF8 encoding.
        /// </summary>
        public static byte[] GZip(this string data) => GZip(data, Encoding.UTF8);

        /// <summary>
        /// Compresses this string into Gzip.
        /// </summary>
        public static byte[] GZip(this string data, Encoding encoding) => encoding.GetBytes(data).GZip();

        /// <summary>
        /// Gets the total size of all files in this directory.
        /// </summary>
        public static long GetSize(this DirectoryInfo folder, bool includeSubDirectories = true)
        {
            return folder.GetFiles(includeSubDirectories).Sum(x => x.AsFile().Length);
        }

        /// <summary>
        /// Gets the size of this folder in human readable text.
        /// </summary>
        public static string GetSizeText(this DirectoryInfo folder, bool includeSubDirectories = true, int round = 1)
        {
            return folder.GetSize(includeSubDirectories).ToFileSizeString(round);
        }

        /// <summary>
        /// Gets the size of this file in human readable text.
        /// </summary>
        public static string GetSizeText(this FileInfo file, int round = 1) => file.Length.ToFileSizeString(round);

        /// <summary>
        /// Detects the characters which are not acceptable in File System and replaces them with a hyphen.
        /// </summary>
        /// <param name="replacement">The character with which to replace invalid characters in the name.</param>
        public static string ToSafeFileName(this string name, char replacement = '-')
        {
            if (name.IsEmpty()) return string.Empty;

            var controlCharacters = name.Where(c => char.IsControl(c));

            var invalidChars = new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }.Concat(controlCharacters);

            foreach (var c in invalidChars)
                name = name.Replace(c, replacement);

            if (replacement.ToString().HasValue())
                name = name.KeepReplacing(replacement.ToString() + replacement, replacement.ToString());

            return name.Summarize(255).TrimEnd("...");
        }

        /// <summary>
        /// Gets a virtual URL to this file. If the file is not in the current website folder it throws an exception.
        /// </summary>
        public static string ToVirtualPath(this FileInfo file)
        {
            if (!file.FullName.StartsWith(AppDomain.CurrentDomain.BaseDirectory, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The file " + file.FullName + " is not in the current website folder.");

            var path = "/" + file.FullName.Substring(AppDomain.CurrentDomain.BaseDirectory.Length).TrimStart("\\").TrimStart("/");
            return path.Replace("\\", "/");
        }

        /// <summary>
        /// Executes this EXE file and returns the standard output.
        /// </summary>
        public static string Execute(this FileInfo exeFile, string args, bool waitForExit = true)
        {
            return Execute(exeFile, args, waitForExit, null);
        }

        /// <summary>
        /// Executes this EXE file and returns the standard output.
        /// </summary>
        public static string Execute(this FileInfo exeFile, string args, bool waitForExit, Action<Process> configuration)
        {
            var output = new StringBuilder();

            var process = new Process
            {
                EnableRaisingEvents = true,

                StartInfo = new ProcessStartInfo
                {
                    FileName = exeFile.FullName,
                    Arguments = args,
                    WorkingDirectory = exeFile.Directory.FullName,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas",
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            configuration?.Invoke(process);

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data.HasValue()) lock (output) output.AppendLine(e.Data);
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null) lock (output) output.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (waitForExit)
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception($"External command failed: '{exeFile.FullName}':\r\n{output}");
            }

            return output.ToString();
        }

        /// <summary>
        /// Gets the mime type based on the file extension.
        /// </summary>
        public static string GetMimeType(this FileInfo file)
        {
            switch (file.Extension.OrEmpty().TrimStart("."))
            {
                case "doc": case "docx": return "application/msword";
                case "pdf": return "application/pdf";
                case "ppt": return "application/powerpoint";
                case "rtf": return "application/rtf";
                case "gz": return "application/x-gzip";
                case "zip": return "application/zip";
                case "mpga": case "mp2": return "audio/mpeg";
                case "ram": return "audio/x-pn-realaudio";
                case "ra": return "audio/x-realaudio";
                case "wav": return "audio/x-wav";
                case "svg": return "image/svg+xml";
                case "gif": return "image/gif";
                case "jpeg": case "jpg": case "jpe": return "image/jpeg";
                case "png": return "image/png";
                case "tiff": case "tif": return "image/tiff";
                case "html": case "htm": return "text/html";
                case "txt": return "text/plain";
                case "mpeg": case "mpg": case "mpe": return "video/mpeg";
                case "mov": case "qt": return "video/quicktime";
                case "avi": return "video/avi";
                case "mid": return "audio/mid";
                case "midi": return "application/x-midi";
                case "divx": return "video/divx";
                case "webm": return "video/webm";
                case "wma": return "audio/x-ms-wma";
                case "mp3": return "audio/mp3";
                case "ogg": return "audio/ogg";
                case "rma": return "audio/rma";
                case "mp4": return "video/mp4";
                case "wmv": return "video/x-ms-wmv";
                case "f4v": return "video/x-f4v";
                case "ogv": return "video/ogg";
                case "3gp": return "video/3gpp";
                default: return "application/octet-stream";
            }
        }

        /// <summary>
        /// Gets the files in this folder. If this folder is null or non-existent it will return an empty array.
        /// </summary>
        public static IEnumerable<FileInfo> GetFilesOrEmpty(this DirectoryInfo folder, string searchPattern)
        {
            if (folder == null || !folder.Exists())
                return Enumerable.Empty<FileInfo>();

            return folder.GetFiles(searchPattern);
        }

        public static async Task<string> ReadAllTextAsync(this FileInfo file)
        {
            using (var reader = File.OpenText(file.FullName))
                return await reader.ReadToEndAsync();
        }

        public static async Task<byte[]> ReadAllBytesAsync(this FileInfo file)
        {
            using (var reader = File.OpenRead(file.FullName))
            {
                var result = new byte[file.Length];
                await reader.ReadAsync(result, 0, result.Length);
                return result;
            }
        }

        /// <param name="encoding">By default it will be UTF-8</param>
        public static async Task WriteAllTextAsync(this FileInfo file, string content, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            }

            using (var writer = new StreamWriter(file.FullName, append: false, encoding: encoding))
                await writer.WriteAsync(content);
        }

        public static async Task WriteAllBytesAsync(this FileInfo file, byte[] data)
        {
            using (var writer = File.Create(file.FullName))
                await writer.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// If this file exists, it will simply return it. 
        /// Otherwise it will throw a FileNotFoundException with the message of 'File not found: {path}'.
        /// </summary>
        public static FileInfo ExistsOrThrow(this FileInfo file)
        {
            if (!file.Exists())
                throw new FileNotFoundException("File not found: " + file.FullName);

            return file;
        }

        /// <summary>
        /// If this directory exists, it will simply return it. 
        /// Otherwise it will throw a DirectoryNotFoundException with the message of 'Directory not found: {path}'.
        /// </summary>
        public static DirectoryInfo ExistsOrThrow(this DirectoryInfo directory)
        {
            if (!directory.Exists())
                throw new DirectoryNotFoundException("Directory not found: " + directory.FullName);

            return directory;
        }
    }
}