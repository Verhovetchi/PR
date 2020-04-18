using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MSharp.Framework
{
    public class DefaultApplicationEventManager : DefaultApplicationEventManagerBase
    {
        /// <summary>
        /// Gets the IP address of the current user.
        /// </summary>
        public override string GetCurrentUserIP()
        {
            try
            {
                return HttpContext.Current?.Request.UserHostAddress;
            }
            catch (Exception err)
            {
                Debug.WriteLine("Cannot get Current user IP:" + err);
                return null;
            }
        }

        protected override IPrincipal CurrentUser => HttpContext.Current?.User;
    }
}
