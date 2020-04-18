namespace System
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using MSharp.Framework;
    using MSharp.Framework.Services;
    using MSharp.Framework.Services.Globalization;
    using Xml.Linq;

    partial class MSharpExtensions
    {
        /// <summary>
        /// Adds the specified query string setting to this Url.
        /// </summary>
        public static Uri AddQueryString(this Uri url, string key, string value)
        {
            var qs = url.GetQueryString();

            qs.RemoveWhere(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

            qs.Add(key, value);

            return url.ReplaceQueryString(qs);
        }

        /// <summary>
        /// Gets the query string parameters of this Url.
        /// </summary>
        public static Dictionary<string, string> GetQueryString(this Uri url)
        {
            var entries = System.Web.HttpUtility.ParseQueryString(url.Query);
            return entries.AllKeys.ExceptNull().ToDictionary(a => a.ToLower(), a => entries[a]);
        }

        /// <summary>
        /// Removes the specified query string parameter.
        /// </summary>
        public static Uri RemoveEmptyQueryParameters(this Uri url)
        {
            var toRemove = url.GetQueryString().Where(x => x.Value.IsEmpty()).ToList();

            foreach (var item in toRemove) url = url.RemoveQueryString(item.Key);

            return url;
        }

        /// <summary>
        /// Removes the specified query string parameter.
        /// </summary>
        public static Uri RemoveQueryString(this Uri url, string key)
        {
            var qs = url.GetQueryString();
            key = key.ToLower();
            if (qs.ContainsKey(key)) qs.Remove(key);

            return url.ReplaceQueryString(qs);
        }

        /// <summary>
        /// Removes all query string parameters of this Url and instead adds the specified ones.
        /// </summary>
        public static Uri ReplaceQueryString(this Uri baseUrl, Dictionary<string, string> queryStringDictionary)
        {
            var r = new StringBuilder();

            r.Append(baseUrl.Scheme);

            r.Append("://");

            r.Append(baseUrl.Host);

            if (baseUrl.Port != 80 && baseUrl.Port != 443) r.Append(":" + baseUrl.Port);

            r.Append(baseUrl.AbsolutePath);

            var query = queryStringDictionary.Select(a => "{0}={1}".FormatWith(a.Key, a.Value.UrlEncode())).ToString("&");

            if (query.HasValue())
            {
                r.Append("?");
                r.Append(query);
            }

            return new Uri(r.ToString());
        }

        /// <summary>
        /// Gets the root of the requested website.
        /// </summary>
        public static string GetWebsiteRoot(this Uri requestUrl)
        {
            var result = requestUrl.Scheme + "://" + requestUrl.DnsSafeHost;
            if (requestUrl.Port != 80 && requestUrl.Port != 443) result += ":" + requestUrl.Port;
            result += "/";
            return result;
        }

        /// <summary>
        /// Downloads the text in this URL.
        /// </summary>
        public static string Download(this Uri url, string cookieValue = null, int timeOut = 60000)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);

            request.Timeout = timeOut;

            if (cookieValue.HasValue())
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.SetCookies(url, cookieValue.OrEmpty());
            }

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                    return stream.ReadAllText();
            }
        }

        /// <summary>
        /// Downloads the data in this URL.
        /// </summary>
        public static byte[] DownloadData(this Uri url, string cookieValue = null, int timeOut = 60000)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);

            request.Timeout = timeOut;

            if (cookieValue.HasValue())
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.SetCookies(url, cookieValue.OrEmpty());
            }

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                    return stream.ReadAllBytes();
            }
        }

        /// <summary>
        /// Downloads the data in this URL.
        /// </summary>
        public static Document DownloadDocument(this Uri url, string cookieValue = null, int timeOut = 60000)
        {
            var fileName = "File.Unknown";

            if (url.IsFile)
            {
                fileName = url.ToString().Split('/').Last();
            }

            return new Document(url.DownloadData(cookieValue, timeOut), fileName);
        }

        /// <summary>
        /// Reads all text in this stream as UTF8.
        /// </summary>
        public static string ReadAllText(this Stream response)
        {
            var result = "";

            // Pipes the stream to a higher level stream reader with the required encoding format. 
            using (var readStream = new StreamReader(response, Encoding.UTF8))
            {
                var read = new char[256];
                // Reads 256 characters at a time.
                var count = readStream.Read(read, 0, 256);

                while (count > 0)
                {
                    result += new string(read, 0, count);
                    count = readStream.Read(read, 0, 256);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the translation of this object's string representation.
        /// </summary>
        public static string ToString(this IEntity instance, ILanguage language)
        {
            if (language == null)
                throw new ArgumentNullException("language");

            return Translator.Translate(instance.ToString(), language);
        }

        /// <summary>
        /// Gets the Html Encoded version of this text.
        /// </summary>
        public static string HtmlEncode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            return HttpUtility.HtmlEncode(text);
        }

        /// <summary>
        /// Gets the Html Decoded version of this text.
        /// </summary>
        public static string HtmlDecode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            return HttpUtility.HtmlDecode(text);
        }

        /// <summary>
        /// Gets the Url Encoded version of this text.
        /// </summary>
        public static string UrlEncode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            return HttpUtility.UrlEncode(text);
        }

        /// <summary>
        /// Gets the Url Decoded version of this text.
        /// </summary>
        public static string UrlDecode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            return HttpUtility.UrlDecode(text);
        }

        /// <summary>
        /// Properly sets a query string key value in this Uri, returning a new Uri object.
        /// </summary>
        public static Uri SetQueryString(this Uri uri, string key, object value)
        {
            var valueString = string.Empty;

            if (value != null)
            {
                if (value is IEntity)
                {
                    valueString = (value as IEntity).GetId().ToString();
                }
                else
                {
                    valueString = value.ToString();
                }
            }

            var pairs = HttpUtility.ParseQueryString(uri.Query);

            pairs[key] = valueString;

            var builder = new UriBuilder(uri) { Query = pairs.ToString() };

            return builder.Uri;
        }
    }
}