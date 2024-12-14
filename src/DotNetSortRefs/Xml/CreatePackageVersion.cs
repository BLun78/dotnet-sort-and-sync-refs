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

            var elementsOfProjectFiles = new List<XElement>();
            foreach (var projFile in fileProjects)
            {
                var docProjFile = XDocument.Parse(await fileSystem.File.ReadAllTextAsync(projFile).ConfigureAwait(false));
                var itemGroups = docProjFile.XPathSelectElements($"//ItemGroup[{ConstConfig.ProjectElementTypes}]");
                elementsOfProjectFiles.AddRange(itemGroups);
            }
            var referenceElementsOfProjectFiles = elementsOfProjectFiles.GetReferenceElements();
            foreach (var element in referenceElementsOfProjectFiles)
            {
                itemGroup.Add(element);

            }

            await using Stream sw = new FileStream(fileSystem.Path.Combine(path, @"Directory.Packages.props"), FileMode.OpenOrCreate);
            await doc.SaveAsync(sw, SaveOptions.None, CancellationToken.None);
            await sw.FlushAsync().ConfigureAwait(false);

            return result;
        }
    }
}
