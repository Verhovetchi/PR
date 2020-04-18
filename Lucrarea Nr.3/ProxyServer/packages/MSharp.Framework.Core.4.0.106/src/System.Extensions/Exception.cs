namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using MSharp.Framework;
    using MSharp.Framework.Services;
    using Web;

    partial class MSharpExtensions
    {
        public static List<Action<Exception, StringBuilder>> ToFullMessageExtendedTypeChecking = new List<Action<Exception, StringBuilder>>();

        public static string ToFullMessage(this Exception ex)
        {
            return ToFullMessage(ex, additionalMessage: null, includeStackTrace: false, includeSource: false, includeData: false);
        }

        /// <summary>
        /// Returns a more complete text dump of this exception, than just its text.
        /// </summary>
        public static string ToFullMessage(this Exception error, string additionalMessage, bool includeStackTrace, bool includeSource, bool includeData)
        {
            if (error == null) throw new NullReferenceException("This exception object is null");

            var r = new StringBuilder();
            r.AppendLineIf(additionalMessage, additionalMessage.HasValue());
            var err = error;
            while (err != null)
            {
                r.AppendLine(err.Message);
                if (includeData && err.Data != null)
                {
                    r.AppendLine("\r\nException Data:\r\n{");
                    foreach (var i in err.Data)
                        r.AppendLine(ToLogText(i).WithPrefix("    "));

                    r.AppendLine("}");
                }

                if (err is ReflectionTypeLoadException)
                {
                    foreach (var loaderEx in (err as ReflectionTypeLoadException).LoaderExceptions)
                        r.AppendLine("Type load exception: " + loaderEx.ToFullMessage());
                }

                foreach (var action in ToFullMessageExtendedTypeChecking)
                    action(err, r);
                //try
                //{
                //    r.AppendLineIf((err as HttpUnhandledException)?.GetHtmlErrorMessage().TrimBefore("Server Error"));
                //}
                //catch
                //{
                //    // No logging is needed
                //}

                err = err.InnerException;

                if (err is TargetInvocationException)
                    err = err.InnerException;

                if (err != null)
                {
                    r.AppendLine();
                    if (includeStackTrace)
                        r.AppendLine("###############################################");
                    r.Append("Base issue: ");
                }
            }

            if (error != null)
                if (includeStackTrace && error.StackTrace.HasValue())
                {
                    var stackLines = error.StackTrace.Or("").Trim().ToLines();
                    stackLines = stackLines.Except(l => l.Trim().StartsWith("at System.Data.")).ToArray();
                    r.AppendLine(stackLines.ToString("\r\n\r\n").WithPrefix("\r\n--------------------------------------\r\nSTACK TRACE:\r\n\r\n"));
                }

            return r.ToString();
        }

        static string ToLogText(object item)
        {
            try
            {
                if (item is DictionaryEntry)
                    return ((DictionaryEntry)item).Get(x => x.Key + ": " + x.Value);
                return item.ToString();
            }
            catch
            {
                return "?";
            }
        }

        /// <summary>
        /// <para>Creates a log-string from the Exception.</para>
        /// <para>The result includes the stacktrace, innerexception et cetera, separated by <seealso cref = "Environment.NewLine"/>.</para>
        /// </summary>
        /// <param name = "ex">The exception to create the string from.</param>
        /// <param name = "additionalMessage">Additional message to place at the top of the string, maybe be empty or null.</param>
        public static string ToLogString(this Exception ex, string additionalMessage)
        {
            var r = new StringBuilder();
            r.AppendLine(ex.ToFullMessage(additionalMessage, includeStackTrace: true, includeSource: true, includeData: true));
            return r.ToString();
        }

        public static string ToLogString(this Exception ex) => ToLogString(ex, null);

        /// <summary>
        /// Adds a piece of data to this exception.
        /// </summary>
        public static Exception AddData(this Exception exception, string key, object value)
        {
            if (value != null)
            {
                try
                {
                    exception.Data.Add(key, value);
                }
                catch
                {
                    // Not serializable
                    try
                    {
                        exception.Data.Add(key, value.ToString());
                    }
                    catch
                    {
                        // No logging is needed
                    }
                }
            }

            return exception;
        }

        public static string GetResponseBody(this WebException ex)
        {
            if (ex.Response == null) return null;
            using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                return reader.ReadToEnd();
        }
    }
}