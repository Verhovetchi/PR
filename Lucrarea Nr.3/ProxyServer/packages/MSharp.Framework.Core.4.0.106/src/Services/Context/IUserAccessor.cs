using MSharp.Framework.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace MSharp.Framework
{
    public interface IUserAccessor
    {
        IPrincipal User { get; }
    }
}
