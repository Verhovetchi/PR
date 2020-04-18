namespace System
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Web;
    using MSharp.Framework;
    using MSharp.Framework.Data;
    using MSharp.Framework.UI;
    using Newtonsoft.Json;
    using Threading.Tasks;

    /// <summary>
    /// Provides extensions methods to Standard .NET types.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class MSharpExtensionsWeb
    {
        /// <summary>
        /// Registers an external script url on this page.
        /// </summary>
        /// <param name="location">Specifies whether the script should be added to the page header. If False, it will be added to the </param>
        public static void RegisterScriptFile(this System.Web.UI.UserControl module, string scriptUrl, ScriptInsertLocation location = ScriptInsertLocation.FormBottom)
        {
            var mSharpPage = module.Page as MSharp.Framework.UI.Page;

            if (mSharpPage == null) throw new InvalidOperationException("The page of this module is not MSharp.Framework.UI.Page.");

            mSharpPage.RegisterScriptFile(scriptUrl, location);
        }

        /// <summary>
        /// It will restart the application.
        /// </summary>
        public static bool Restart(this System.Web.HttpApplication application)
        {
            // Method 1
            try { System.Diagnostics.Process.GetCurrentProcess().Kill(); return true; }
            catch (ThreadAbortException) { return true; /* Good */  }
            catch {  /* We'll try another method */  }

            // Method 2 - needs full trust
            try { HttpRuntime.UnloadAppDomain(); return true; }
            catch (ThreadAbortException) { return true; /* Good */  }
            catch {  /* We'll try another method */  }

            // Method 3 - try web.config
            try { File.SetLastWriteTimeUtc(AppDomain.CurrentDomain.GetPath("web.config"), DateTime.UtcNow); return true; }
            catch (ThreadAbortException) { return true; /* Good */  }
            catch { return false; /*Still no hope, you have to do something else.*/            }
        }

        /// <summary>
        /// Initializes a new Document instance, for the specified posted file.
        /// </summary>
        public static Document ToDocument(this HttpPostedFile @this)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));

            var fileName = Path.GetFileName(@this.FileName);

            var newFileData = new byte[@this.InputStream.Length];
            @this.InputStream.Position = 0;
            @this.InputStream.Read(newFileData, 0, newFileData.Length);

            return new Document(newFileData, fileName);
        }
    }
}
