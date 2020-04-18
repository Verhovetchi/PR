using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSharp.Framework.Services.Globalization
{
    public class CookiePropertyHelper : ICookiePropertyHelper
    {
        public T Get<T>() => CookieProperty.Get<T>();
    }
}
