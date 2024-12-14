using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DotNetSortRefs
{
    internal class SyncRefs
    {
        public const string PackageReference = @"PackageReference";
        public const string Reference = @"Reference";
        public const string Include = @"Include";
        public static string ProjectElementTypes = $"{PackageReference}|{Reference}";
        public const string PropsElementTypes = @"PackageVersion";

        public static async Task<int> CleanUp(IEnumerable<string> projFiles, IEnumerable<string> propsFiles)
        {
            var elementsOfProjectFiles = new List<XElement>();
            foreach (var projFile in projFiles)
            {
                var docProjFile = XDocument.Parse(await System.IO.File.ReadAllTextAsync(projFile).ConfigureAwait(false));
                var itemGroups = docProjFile.XPathSelectElements($"//ItemGroup[{ProjectElementTypes}]");
                elementsOfProjectFiles.AddRange(itemGroups);
            }
            var referenceElementsOfProjectFiles = GetReferenceElements(elementsOfProjectFiles);

            var lookup = referenceElementsOfProjectFiles.ToLookup(x => x.FirstAttribute.Value, element => element);
            var removeList = new List<XElement>();
            foreach (var propsFile in propsFiles)
            {

                removeList.Clear();

                var docPropsFile = XDocument.Parse(await System.IO.File.ReadAllTextAsync(propsFile).ConfigureAwait(false));

                var itemGroups = docPropsFile.XPathSelectElements($"//ItemGroup[{PropsElementTypes}]");
                var attributesOfPropsFiles = GetReferenceElements(itemGroups.ToList());

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

                foreach (var element in removeList)
                {
                    element.Remove();
                }

                await using Stream sw = new FileStream(propsFile, FileMode.Truncate);
                await docPropsFile.SaveAsync(sw, SaveOptions.None, default);
                await sw.FlushAsync().ConfigureAwait(false);
            }

            return 1;
        }

        private static List<XElement> GetReferenceElements(List<XElement> elementsOfProjectFiles)
        {
            var attributesOfProjectFiles = new List<XElement>();

            foreach (var elementsOfProjectFile in elementsOfProjectFiles)
            {
                XElement node = null;
                do
                {
                    if (node == null)
                    {
                        node = elementsOfProjectFile.FirstNode as XElement;
                    }
                    else
                    {
                        node = node.NextNode as XElement;
                    }
                    var firstAttribute = node.FirstAttribute;
                    attributesOfProjectFiles.Add(node);


                } while (node.NextNode != null);

            }
            return attributesOfProjectFiles;
        }
    }
}
