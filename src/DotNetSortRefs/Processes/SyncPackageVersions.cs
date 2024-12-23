using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Extensions;
using DotnetSortAndSyncRefs.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Processes
{
    internal class SyncPackageVersions : DryRun
    {
        private readonly IServiceProvider _serviceProvider;
        
        public SyncPackageVersions(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<int> RemovePackageVersions(
            IEnumerable<string> projFiles,
            IEnumerable<string> propsFiles)
        {
            var result = ErrorCodes.SortingIsFailed;

            // collect Project references
            var elementsOfProjectFiles = new List<XElement>();
            foreach (var projFile in projFiles)
            {
                var xmlProjectFile = _serviceProvider.GetRequiredService<XmlProjectFile>();
                elementsOfProjectFiles.AddRange(xmlProjectFile.ItemGroups);
            }
            var referenceElementsOfProjectFiles = elementsOfProjectFiles.GetReferenceElements();

            var lookup = referenceElementsOfProjectFiles.ToLookup(x => x.FirstAttribute?.Value, element => element);
            var removeList = new List<XElement>();

            foreach (var propsFile in propsFiles)
            {
                removeList.Clear();

                var xmlCentralPackageManagementFile = _serviceProvider.GetRequiredService<XmlCentralPackageManagementFile>();
                await xmlCentralPackageManagementFile.LoadFileAsync(propsFile, IsDryRun).ConfigureAwait(false);

                var attributesOfPropsFiles = xmlCentralPackageManagementFile.ItemGroups.ToList().GetReferenceElements();

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
                    await xmlCentralPackageManagementFile.SaveAsync();
                }

                result = ErrorCodes.Ok;
            }

            return result;
        }

    }
}
