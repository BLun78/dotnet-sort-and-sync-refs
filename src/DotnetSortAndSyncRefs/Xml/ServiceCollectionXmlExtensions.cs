using DotnetSortAndSyncRefs.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Xml
{
    internal static class ServiceCollectionXmlExtensions
    {
        /// <summary>
        /// All XML File Types are registered with this
        /// </summary>
        /// <param name="serviceCollection">serviceCollection</param>
        /// <returns>serviceCollection</returns>
        public static IServiceCollection AddXmlFiles(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<XmlAllElementFile>()
                .AddTransient<XmlProjectFile>()
                .AddTransient<XmlCentralPackageManagementFile>();
        }
    }
}
