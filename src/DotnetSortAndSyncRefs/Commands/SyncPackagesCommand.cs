using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Xml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotnetSortAndSyncRefs.Extensions;
using McMaster.Extensions.CommandLineUtils;

namespace DotnetSortAndSyncRefs.Commands
{
    [Command("sync", "sy",
        Description = "Syncs package references in all project files.")]
    internal class SyncPackagesCommand : SortReferences, ICommandBase
    {
        public SyncPackagesCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "sync")
        {
        }

        public override async Task<int> OnExecute()
        {
            var result = ErrorCodes.SyncPackagesFailed;

            // collect Project references
            var elementsOfProjectFiles = new List<XElement>();
            foreach (var projFile in FileProjects)
            {
                var xmlProjectFile = ServiceProvider.GetRequiredService<XmlProjectFile>();

                await xmlProjectFile
                    .LoadFileAsync(projFile, IsDryRun)
                    .ConfigureAwait(false);

                xmlProjectFile.FixAndGroupItemGroups();

                if (IsNoDryRun)
                {
                    await xmlProjectFile
                        .SaveAsync()
                        .ConfigureAwait(false);
                }

                elementsOfProjectFiles.AddRange(xmlProjectFile.ItemGroups);
            }

            var referenceElementsOfProjectFiles = elementsOfProjectFiles.GetReferenceElements();

            var lookup = referenceElementsOfProjectFiles
                .ToLookup(x => x.FirstAttribute?.Value, element => element);
            var removeList = new List<XElement>();

            if (FileProps.Count == 0)
            {
                Reporter.NotOk("No Central Package Management File was found. Nothing to Sync");
                return ErrorCodes.SyncPackagesFailed;
            }

            foreach (var propsFile in FileProps)
            {
                removeList.Clear();

                var xmlCentralPackageManagementFile = ServiceProvider.GetRequiredService<XmlCentralPackageManagementFile>();
                await xmlCentralPackageManagementFile
                    .LoadFileAsync(propsFile, IsDryRun)
                    .ConfigureAwait(false);

                xmlCentralPackageManagementFile.FixAndGroupItemGroups();

                if (IsNoDryRun)
                {
                    await xmlCentralPackageManagementFile
                        .SaveAsync()
                        .ConfigureAwait(false);
                }

                var attributesOfPropsFiles = xmlCentralPackageManagementFile
                    .ItemGroups
                    .ToList()
                    .GetReferenceElements();

                // compare project references and PackageVersion
                foreach (var attributesOfPropsFile in attributesOfPropsFiles)
                {
                    var name = attributesOfPropsFile.FirstAttribute?.Value;
                    if (name != null)
                    {
                        var finding = lookup[name];
                        if (!finding.Any())
                        {
                            removeList.Add(attributesOfPropsFile);
                        }
                    }
                }

                // remove PackageVersion
                foreach (var element in removeList)
                {
                    element.Remove();
                }

                // write file
                if (IsNoDryRun)
                {
                    await xmlCentralPackageManagementFile
                        .SaveAsync()
                        .ConfigureAwait(false);
                }

                result = ErrorCodes.Ok;
            }

            if (result == ErrorCodes.Ok)
            {
                return await base
                    .OnExecute()
                    .ConfigureAwait(false);
            }

            return result;
        }
    }
}
