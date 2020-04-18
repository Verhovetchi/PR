using System;
using System.Net;
using System.Text;

namespace MSharp.Framework
{
    public class HttpDocumentStorageProvider : IDocumentStorageProvider
    {
        static string BaseUrl => Config.Get("HttpDocumentStorageProvider.BaseUrl");
        static string Password => Config.Get("HttpDocumentStorageProvider.Password");

        static string GetParams(Document doc) => $"?folder={doc.FolderName}&file={GetFileName(doc)}&pass={Password}";

        static string GetFileName(Document doc) => doc.GetFileNameWithoutExtension() + doc.FileExtension;

        static string GenerateSignature(string data) => (data.CreateSHA1Hash() + Password).CreateSHA1Hash();

        public void Delete(Document document) => InvokeGet("delete", document);

        public void Save(Document document)
        {
            try
            {
                var url = BaseUrl.EnsureEndsWith("/save" + GetParams(document));
                var signature = GenerateSignature(document.FileData.ToBase64String());

                var x = document.FileData.ToString();

                using (var client = new WebClient())
                {
                    client.Headers.Add("signature", signature);
                    var result = client.UploadData(url, document.FileData); //.ToString(Encoding.UTF8);

                    //if (result.HasValue()) throw new Exception(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload the file '{document.FileName}' to the file server.", ex);
            }
        }

        public bool FileExists(Document document) => InvokeGet("exists", document).ToString(Encoding.UTF8).To<bool>();

        public byte[] Load(Document document) => InvokeGet("load", document);

        static byte[] InvokeGet(string command, Document doc)
        {
            var url = BaseUrl.EnsureEndsWith("/" + command + GetParams(doc));
            using (var client = new WebClient())
            {
                client.Headers.Add("signature", GenerateSignature(GetFileName(doc)));

                return client.DownloadData(url);
            }
        }
    }
}