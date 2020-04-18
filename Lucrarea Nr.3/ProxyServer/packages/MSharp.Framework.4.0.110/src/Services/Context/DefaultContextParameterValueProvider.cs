using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MSharp.Framework
{
    public class DefaultContextParameterValueProvider : IContextParameterValueProvider
    {
        public string Param(string key) => HttpContext.Current?.Request[key] ?? "";
    }
}
