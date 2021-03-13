using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Wabbajack.Lib;

namespace Wabbajack.App.Services
{
    public static class Extensions
    {
        public static void RegisterAllTypes<T>(this IServiceCollection services, Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var typesFromAssemblies = assembly.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T)));
            foreach (var type in typesFromAssemblies)
            {
                if (type.IsAbstract) continue;
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
            }
        }
        
        public static void RegisterAllVMs(this IServiceCollection services, Assembly assembly,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var typesFromAssemblies = assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(ViewModel)));
            foreach (var type in typesFromAssemblies)
                services.Add(new ServiceDescriptor(type, type, lifetime));
        }
        
    }
}
