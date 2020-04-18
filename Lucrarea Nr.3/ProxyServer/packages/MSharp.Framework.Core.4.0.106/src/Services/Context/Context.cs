using MSharp.Framework.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace MSharp.Framework
{
    public class Context
    {
        public static Context Current
        {
            get => current ?? throw GetNotInitializedException();
            private set => current = value;
        }

        static Exception GetNotInitializedException()
        {
            return new InvalidOperationException($"{nameof(Context)} is not initialized yet. " +
                $"Call '{nameof(Context)}.{nameof(Initialize)}' to initialize it. Also, you can use 'DefaultServiceProvider' as the arrgument.");
        }

        readonly IServiceProvider ServiceProvider;
        private static Context current;

        public IPrincipal User => GetRequiredService<IUserAccessor>().User;

        public IHttpContextItemsAccessor HttpContextItemsAccessor => GetRequiredService<IHttpContextItemsAccessor>();

        public IDictionary HttpContextItems => GetRequiredService<IHttpContextItemsAccessor>().Items;

        Context(IServiceProvider provider) => ServiceProvider = provider;

        public static void Initialize(IServiceProvider provider) => Current = new Context(provider);

        public T GetRequiredService<T>() => ServiceProvider.GetRequiredService<T>();

        public T GetService<T>() => ServiceProvider.GetService<T>();

        public string Param(string key) => GetRequiredService<IContextParameterValueProvider>().Param(key);
    }
}
