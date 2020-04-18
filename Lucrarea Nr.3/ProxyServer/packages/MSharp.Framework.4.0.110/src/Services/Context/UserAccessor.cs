using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MSharp.Framework
{
    public class UserAccessor : IUserAccessor
    {
        public IPrincipal User => HttpContext.Current?.User;
    }
}
