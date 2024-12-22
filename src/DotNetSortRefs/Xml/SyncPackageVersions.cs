using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using DotnetSortAndSyncRefs.Common;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Packaging;

namespace DotnetSortAndSyncRefs.Xml
{
    internal static class SyncPackageVersions
    {
        public static async Task<int> RemovePackageVersions(
            this IServiceProvider serviceProvider,
            IEnumerable<string> projFiles,
            IEnumerable<string> propsFiles,
            bool dryRun)
        {
            var result = 4;

            // collect Project references
            var elementsOfProjectFiles = new List<XElement>();
            foreach (var projFile in projFiles)
            {
                var xmlProjectFile = serviceProvider.GetRequiredService<XmlProjectFile>();
                elementsOfProjectFiles.AddRange(xmlProjectFile.ItemGroups);
            }
            var referenceElementsOfProjectFiles = elementsOfProjectFiles.GetReferenceElements();

            var lookup = referenceElementsOfProjectFiles.ToLookup(x => x.FirstAttribute?.Value, element => element);
            var removeList = new List<XElement>();
            foreach (var propsFile in propsFiles)
            {

                removeList.Clear();

                var xmlCentralPackageManagementFile = serviceProvider.GetRequiredService<XmlCentralPackageManagementFile>();
                await xmlCentralPackageManagementFile.LoadFileAsync(propsFile, dryRun).ConfigureAwait(false);

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
                if (!dryRun)
                {
                    await xmlCentralPackageManagementFile.SaveAsync();
                }

                result = 0;
            }

            return result;
        }

    }
}
