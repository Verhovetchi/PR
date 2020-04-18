namespace System
{
    using Collections.Concurrent;
    using Collections.Generic;
    using Linq;
    using Linq.Expressions;
    using Reflection;
    using Runtime.Remoting.Messaging;
    using System.Collections;
    using Threading;
    using Threading.Tasks;

    partial class MSharpExtensionsWeb
    {
        public delegate T Method<out T>();

        public static T Cache<T>(this MethodBase method, object instance, object[] arguments, Method<T> methodBody) where T : class
        {
            var key = method.DeclaringType.GUID + ":" + method.Name;
            if (instance != null)
                key += instance.GetHashCode() + ":";

            arguments?.Do(arg => key += arg.GetHashCode() + ":");

            if (CallContext.GetData(key) == null)
            {
                var result = methodBody?.Invoke();
                CallContext.SetData(key, result);
                return result;
            }

            return CallContext.GetData(key) as T;
        }

        public static T Cache<T>(this MethodBase method, object[] arguments, Method<T> methodBody) where T : class
        {
            return Cache<T>(method, null, arguments, methodBody);
        }
    }
}
