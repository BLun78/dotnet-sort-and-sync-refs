using DotNetSortRefs.Common;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace DotNetSortRefs.Xml
{
    internal static class XmlHelper
    {
        public static async Task<List<string>> Inspect(this IFileSystem fileSystem, IEnumerable<string> projFiles)
        {
            var projFilesWithNonSortedReferences = new List<string>();


            foreach (var projFile in projFiles)
            {
                var doc = XDocument.Parse(await fileSystem.File.ReadAllTextAsync(projFile).ConfigureAwait(false));

                var itemGroups = doc.XPathSelectElements($"//ItemGroup[{ConstConfig.AllElementTypes}]");

                foreach (var itemGroup in itemGroups)
                {
                    var references = itemGroup.XPathSelectElements(ConstConfig.AllElementTypes)
                        .Select(x => x.Attribute("Include")?.Value.ToLowerInvariant()).ToList();

                    if (references.Count <= 1) continue;

                    var sortedReferences = references.OrderBy(x => x).ToList();

                    var result = references.SequenceEqual(sortedReferences);

                    if (!result && !projFilesWithNonSortedReferences.Contains(projFile))
                    {
                        projFilesWithNonSortedReferences.Add(projFile);
                    }
                }
            }

            return projFilesWithNonSortedReferences;
        }

        public static XslCompiledTransform GetXslTransform()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("DotNetSortRefs.Sort.xsl");
            using var reader = XmlReader.Create(stream!);
            var xslt = new XslCompiledTransform();
            xslt.Load(reader);
            return xslt;
        }

        public static async Task<int> SortReferences(this IFileSystem fileSystem, Reporter report, IEnumerable<string> projFiles)
        {
            var result = 5;
            var xslt = GetXslTransform();

            foreach (var projFile in projFiles)
            {
                report.Do($"» {projFile}");

                await using var sw = new StringWriter();
                var doc = XDocument.Parse(await fileSystem.File.ReadAllTextAsync(projFile).ConfigureAwait(false));
                xslt.Transform(doc.CreateNavigator(), null, sw);
                await fileSystem.File.WriteAllTextAsync(projFile, sw.ToString()).ConfigureAwait(false);
                result = 0;
            }

            return result;
        }

        public static List<XElement> GetReferenceElements(this List<XElement> elementsOfProjectFiles)
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
                    attributesOfProjectFiles.Add(node);

                } while (node?.NextNode != null);

            }
            return attributesOfProjectFiles;
        }

        public static async Task SaveXDocument(IFileSystem fileSystem, string path, XDocument doc, FileMode fileMode)
        {
            await using Stream sw = new FileStream(path, fileMode);
            await sw.FlushAsync().ConfigureAwait(false);
            await doc.SaveAsync(sw, SaveOptions.None, CancellationToken.None);
            
        }

    }
}
