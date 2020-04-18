namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using MSharp.Framework;
    using MSharp.Framework.Services;
    using Web;

    partial class MSharpExtensionsWeb
    {
        public static IEnumerable<T> Where<T>(this IEnumerable<T> list, string criteria)
        {
            return new DynamicExpressionsCompiler<T>(list).Where(criteria);
        }

        public static IEnumerable Where(this IEnumerable list, string criteria, Type type)
        {
            return new DynamicExpressionsCompiler<object>(list.OfType<object>(), type).Where(criteria);
        }

        public static IEnumerable<K> Select<T, K>(this IEnumerable<T> list, string query)
        {
            return new DynamicExpressionsCompiler<T>(list).Select<K>(query);
        }

        public static IEnumerable<object> Select<T>(this IEnumerable<T> list, string query)
        {
            return Select<T, object>(list, query);
        }
    }
}
