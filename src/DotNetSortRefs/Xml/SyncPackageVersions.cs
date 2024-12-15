using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using DotNetSortRefs.Common;

namespace DotNetSortRefs.Xml
{
    internal static class SyncPackageVersions
    {
        public static async Task<int> RemovePackageVersions(this IFileSystem fileSystem, Reporter report, IEnumerable<string> projFiles, IEnumerable<string> propsFiles)
        {
            var result = 4;

            // collect Project references
            var elementsOfProjectFiles = new List<XElement>();
            foreach (var projFile in projFiles)
            {
                var docProjFile = XDocument.Parse(await fileSystem.File.ReadAllTextAsync(projFile).ConfigureAwait(false));
                var itemGroups = docProjFile.XPathSelectElements($"//ItemGroup[{ConstConfig.ProjectElementTypes}]");
                elementsOfProjectFiles.AddRange(itemGroups);
            }
            var referenceElementsOfProjectFiles = elementsOfProjectFiles.GetReferenceElements();

            var lookup = referenceElementsOfProjectFiles.ToLookup(x => x.FirstAttribute?.Value, element => element);
            var removeList = new List<XElement>();
            foreach (var propsFile in propsFiles)
            {

                removeList.Clear();

                var docPropsFile = XDocument.Parse(await fileSystem.File.ReadAllTextAsync(propsFile).ConfigureAwait(false));

                var itemGroups = docPropsFile.XPathSelectElements($"//ItemGroup[{ConstConfig.PropsElementTypes}]");
                var attributesOfPropsFiles = itemGroups.ToList().GetReferenceElements();

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

                // write new file
                await XmlHelper.SaveXDocument(fileSystem, propsFile, docPropsFile, FileMode.CreateNew);

                result = 0;
            }

            return result;
        }

    }
}
