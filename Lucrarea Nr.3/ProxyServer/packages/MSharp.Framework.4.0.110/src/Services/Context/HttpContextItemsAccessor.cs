using MSharp.Framework.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MSharp.Framework
{
    public class HttpContextItemsAccessor : IHttpContextItemsAccessor
    {
        public bool ItemsAvaiable => HttpContext.Current != null;

        public IDictionary Items => HttpContext.Current?.Items;
    }
}
