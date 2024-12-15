using DotNetSortRefs.Common;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DotNetSortRefs.Xml
{
    internal static class CreatePackageVersion
    {
        private const string WithOutCondition = "WithOutCondition";
        private const string initalFile =
            """
            <Project>
              <PropertyGroup>
                <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
              </PropertyGroup>
              <ItemGroup />
            </Project>
            """;

        public static async Task<int> CreatePackageVersions(this IFileSystem fileSystem,
            List<string> fileProjects,
            string path)
        {
            var result = 3;

            var doc = XDocument.Parse(initalFile);
            var itemGroup = doc.XPathSelectElements($"//ItemGroup").First();

            var dict = new Dictionary<string, XElement>
            {
                { WithOutCondition, itemGroup }
            };

            var elementsOfProjectFiles = new List<XElement>();
            foreach (var projFile in fileProjects)
            {
                var docProjFile = XDocument.Parse(await fileSystem.File.ReadAllTextAsync(projFile).ConfigureAwait(false));

                // search for ItemGroup with ProjectElementTypes and for ItemGroup with ProjectElementTypes|Condition
                var itemGroups = docProjFile
                    .XPathSelectElements($"//ItemGroup[{ConstConfig.ProjectElementTypes}] | //ItemGroup[{ConstConfig.Condition} and {ConstConfig.ProjectElementTypes}]")
                    .ToList();
                CreateItemGroups(itemGroups, itemGroup, dict);

                elementsOfProjectFiles.AddRange(itemGroups);

                var referenceElementsOfProjectFiles = elementsOfProjectFiles.GetReferenceElements();
                foreach (var element in referenceElementsOfProjectFiles)
                {
                    var condition = element.Parent.GetCondition() ?? WithOutCondition;

                    if (dict.TryGetValue(condition, out var value))
                    {
                        var newElement = new XElement(element)
                        {
                            Name = ConstConfig.PropsElementTypes
                        };
                        value.Add(newElement);
                    }
                }
            }
            
            await using Stream sw = new FileStream(fileSystem.Path.Combine(path, @"Directory.Packages.props"), FileMode.OpenOrCreate);
            await doc.SaveAsync(sw, SaveOptions.None, CancellationToken.None);
            await sw.FlushAsync().ConfigureAwait(false);

            return result;
        }

        private static void CreateItemGroups(IEnumerable<XElement> itemGroups, XElement itemGroup, Dictionary<string, XElement> dict)
        {
            foreach (var element in itemGroups)
            {
                var newItemGroup = element.CreateItemGroup(itemGroup);
                if (newItemGroup != null)
                {
                    itemGroup.AddAfterSelf(newItemGroup);
                    dict.Add(newItemGroup.GetCondition(), newItemGroup);
                }
            }
        }

        public static XElement? CreateItemGroup(this XElement inputElement, XElement nodeBeFor)
        {
            var condition = inputElement.GetCondition();

            if (!string.IsNullOrWhiteSpace(condition))
            {
                var element = new XElement("ItemGroup");

                element.SetAttributeValue(ConstConfig.Condition, condition);

                return element;
            }
            return null;
        }

        public static string GetCondition(this XElement element)
        {
            var condition = element.FirstAttribute;
            if (condition != null &&
                condition.Name == ConstConfig.Condition)
            {
                return condition.Value;
            }
            return null;
        }
    }
}
