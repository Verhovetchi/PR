using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace MSharp.Framework
{
    public class SessionMemoryAccessor : ISessionMemoryAccessor
    {
        const string SESSION_KEY = "User.Session.Memory";

        public SessionMemory GetInstance(Func<SessionMemory> factory)
        {
            var session = HttpContext.Current?.Session;

            if (session != null)
            {
                if (session[SESSION_KEY] == null)
                    session[SESSION_KEY] = factory();

                return (SessionMemory)session[SESSION_KEY];
            }
            else // Use thread:
            {
                if (CallContext.GetData(SESSION_KEY) == null)
                    CallContext.SetData(SESSION_KEY, factory());

                return (SessionMemory)CallContext.GetData(SESSION_KEY);
            }
        }
    }
}
