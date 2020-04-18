using MSharp.Framework;
using System.Collections.Generic;
using System.Linq;

namespace System
{
    /// <summary>
    /// Provides process context data sharing mechanism to pass arguments and data around execution in a shared pipeline.
    /// It supports context nesting.
    /// </summary>
    public class ProcessContext<T> : IDisposable
    {


        static readonly object SyncLock = new object();
        static Func<T> DefaultDataExpression = delegate { return default(T); };

        /// <summary>
        /// Creates a new Process Context.
        /// </summary>
        public ProcessContext(T data) : this(null, data) { }

        /// <summary>
        /// Creates a new Process Context with the specified key and data.
        /// </summary>
        public ProcessContext(string key, T data)
        {
            Data = data;
            Key = key;
            GetContexts(key).Add(this);
        }

        static string GetProcessContextKey(string key)
        {
            return "ProcessContext:" + typeof(T).FullName + "|K:" + key;
        }

        /// <summary>
        /// Sets the default data expression, when no context data is available.
        /// </summary>
        public static void SetDefaultDataExpression(Func<T> expression)
        {
            if (expression == null)
                expression = delegate { return default(T); };

            DefaultDataExpression = expression;
        }

        /// <summary>
        /// Gets or sets the Data of this ProcessContext.
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// Gets or sets the key of this ProcessContext.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// A number of nested process context objects in the currenly executing thread.
        /// </summary>
        public static List<ProcessContext<T>> GetContexts(string key)
        {
            lock (SyncLock)
            {
                var contextKey = GetProcessContextKey(key);

                return Context.Current.GetRequiredService<IProcessContextAccessor>()
                    .GetList<T>(contextKey);
            }
        }

        /// <summary>
        /// Gets the data of the current context with default key (null).
        /// </summary>
        public static T Current => GetCurrent(null);

        /// <summary>
        /// Gets the data of the current context with the specified key.
        /// </summary>
        public static T GetCurrent(string key)
        {
            if (GetContexts(key).Count == 0) return DefaultDataExpression();
            return GetContexts(key).Last().Data;
        }

        /// <summary>
        /// Disposes the current process context and switches the actual context to the containing process context.
        /// </summary>
        public void Dispose()
        {
            try { GetContexts(Key).Remove(this); }
            catch { }
        }
    }

    public interface IProcessContextAccessor
    {
        List<ProcessContext<T>> GetList<T>(string key);
    }

    /// <summary>
    /// Provides a facade for easiper creation of a Process Context.
    /// </summary>
    public static class ProcessContext
    {
        /// <summary>
        /// Create a process context for the specified object.
        /// To access the context object, you can use ProcessContext&lt;Your Type&gt;.Current.
        /// </summary>
        public static ProcessContext<T> Create<T>(T contextObject)
        {
            return new ProcessContext<T>(contextObject);
        }

        /// <summary>
        /// Create a process context for the specified object with the specified key.
        /// To access the context object, you can use ProcessContext&lt;Your Type&gt;.GetCurrent(key).
        /// </summary>
        public static ProcessContext<T> Create<T>(string key, T contextObject)
        {
            return new ProcessContext<T>(key, contextObject);
        }
    }
}