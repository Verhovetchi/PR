using System.Web.UI;

namespace MSharp.Framework.UI
{
    /// <summary>
    /// Base Page containing base common functionality, all pages inherit from this.
    /// </summary>
    public class MasterPage : System.Web.UI.MasterPage
    {
        public MasterPage()
        {
            ID = "Root";
        }

        public virtual bool IsModal() => false;

        protected override void Render(HtmlTextWriter writer) => base.Render(writer);

        protected System.Web.UI.Page UnderlyingPage
        {
            get { return base.Page; }
            set { base.Page = value; }
        }

        public new Page Page
        {
            get { return base.Page as Page; }
            set { base.Page = value; }
        }

        /// <summary>
        /// Gets the basic raw url of the login page applicable to this master page.
        /// </summary>
        public virtual string GetLogInUrl() => string.Empty;

        public MessageBoxManager MessageBox => MessageBoxManager.GetMessageBox(UnderlyingPage);
    }
}