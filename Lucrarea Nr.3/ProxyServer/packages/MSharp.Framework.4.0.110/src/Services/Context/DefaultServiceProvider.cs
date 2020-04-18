using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSharp.Framework
{
    public class DefaultServiceProvider : IServiceProvider
    {
        readonly IServiceCollection Services;
        ServiceProvider Provider;

        public DefaultServiceProvider()
        {
            Services = new ServiceCollection();
            Services.AddSingleton<IUserAccessor, UserAccessor>()
                .AddSingleton<IHttpContextItemsAccessor, HttpContextItemsAccessor>()
                .AddSingleton<IContextParameterValueProvider, DefaultContextParameterValueProvider>()
                .AddSingleton<IProcessContextAccessor, ProcessContextAccessor>();
        }

        public DefaultServiceProvider AddService(params ServiceDescriptor[] services)
        {
            Services.AddRange(services);
            return this;
        }

        public object GetService(Type serviceType) => GetProvider().GetService(serviceType);

        ServiceProvider GetProvider() => Provider ?? (Provider = Services.BuildServiceProvider());
    }
}
