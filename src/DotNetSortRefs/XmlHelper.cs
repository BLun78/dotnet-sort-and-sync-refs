using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace DotNetSortRefs
{
    internal static class XmlHelper
    {
        public const string ElementTypes = "PackageReference|Reference|PackageVersion";

        public static async Task<List<string>> Inspect(IEnumerable<string> projFiles)
        {
            var projFilesWithNonSortedReferences = new List<string>();


            foreach (var projFile in projFiles)
            {
                var doc = XDocument.Parse(await System.IO.File.ReadAllTextAsync(projFile).ConfigureAwait(false));

                var itemGroups = doc.XPathSelectElements($"//ItemGroup[{ElementTypes}]");

                foreach (var itemGroup in itemGroups)
                {
                    var references = itemGroup.XPathSelectElements(ElementTypes)
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

        public static async Task<int> SortReferences(IEnumerable<string> projFiles, IReporter report)
        {
            var xslt = XmlHelper.GetXslTransform();

            foreach (var projFile in projFiles)
            {
                report.Output($"» {projFile}");

                await using var sw = new StringWriter();
                var doc = XDocument.Parse(await System.IO.File.ReadAllTextAsync(projFile).ConfigureAwait(false));
                xslt.Transform(doc.CreateNavigator(), null, sw);
                await File.WriteAllTextAsync(projFile, sw.ToString()).ConfigureAwait(false);
            }

            return 0;
        }
    }
}
