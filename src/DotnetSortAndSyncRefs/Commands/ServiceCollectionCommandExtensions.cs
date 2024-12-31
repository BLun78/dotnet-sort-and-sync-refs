using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Commands
{
    internal static class ServiceCollectionXmlExtensions
    {
        /// <summary>
        /// All Commands are registered with this
        /// </summary>
        /// <param name="serviceCollection">serviceCollection</param>
        /// <returns>serviceCollection</returns>
        public static IServiceCollection AddCommands(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<CentralPackageManagementCommand>()
                .AddSingleton<DotnetUpgradeCommand>()
                .AddSingleton<InspectorCommand>()
                .AddSingleton<NuGetUpdateCommand>()
                .AddSingleton<SortReferencesCommand>()
                .AddSingleton<SyncPackagesCommand>();

        }
    }
}
