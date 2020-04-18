using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MSharp.Framework
{
    public class ProcessContextAccessor : IProcessContextAccessor
    {
        public List<ProcessContext<T>> GetList<T>(string key)
        {
            if (CallContext.GetData(key) == null)
            {
                var result = new List<ProcessContext<T>>();

                CallContext.SetData(key, result);
                return result;
            }
            else
            {
                // Already exists:
                return CallContext.GetData(key) as List<ProcessContext<T>>;
            }
        }
    }
}
