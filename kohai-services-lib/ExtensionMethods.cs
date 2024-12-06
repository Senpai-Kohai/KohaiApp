using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kohai.Attributes;
using Kohai.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kohai
{
    public static class ExtensionMethods
    {
        public static ServiceCollection AddKohaiServices(this ServiceCollection services)
        {
            // add all service types to service registry, so forms/etc can easily use them via constructors
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsNotPublic || type.IsValueType || !type.IsAssignableTo(typeof(IService)))
                        continue;

                    try
                    {
                        services.AddSingleton(type);
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine($"An exception occurred trying to add service configuration type [{type.Name}] to the service provider: {exc}.");
                    }
                }
            }

            return services;
        }

        public static ServiceCollection AddKohaiServiceConfiguration(this ServiceCollection services)
        {
            // add all configuration types to service registry, so forms/etc can easily use them via constructors
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsNotPublic || type.IsValueType)
                        continue;

                    try
                    {
                        var configAttr = type.GetCustomAttribute<ServiceConfigurationAttribute>();
                        if (configAttr == null)
                            continue;

                        var serviceConfig = Activator.CreateInstance(type);
                        if (serviceConfig == null)
                            continue;

                        Debug.WriteLine($"Adding service configuration type [{type.Name}] directly to the service provider.");

                        Kohai.Configuration.IConfiguration.LoadConfiguration(serviceConfig, configAttr?.SectionName);
                        services.AddSingleton(type, serviceConfig);
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine($"An exception occurred trying to add service configuration type [{type.Name}] to the service provider: {exc}.");
                    }
                }
            }

            return services;
        }
    }
}
