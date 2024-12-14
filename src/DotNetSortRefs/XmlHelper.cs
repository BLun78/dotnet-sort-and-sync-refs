using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Xsl;
using System.Xml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using McMaster.Extensions.CommandLineUtils;

namespace DotNetSortRefs
{
    internal static class XmlHelper
    {
        public static async Task<List<string>> Inspect(IEnumerable<string> projFiles)
        {
            var projFilesWithNonSortedReferences = new List<string>();

            foreach (var proj in projFiles)
            {
                await using var sw = new StringWriter();

                var doc = XDocument.Parse(await System.IO.File.ReadAllTextAsync(proj).ConfigureAwait(false));

                const string elementTypes = "PackageReference|Reference|PackageVersion";
                var itemGroups = doc.XPathSelectElements($"//ItemGroup[{elementTypes}]");

                foreach (var itemGroup in itemGroups)
                {
                    var references = itemGroup.XPathSelectElements(elementTypes)
                        .Select(x => x.Attribute("Include")?.Value.ToLowerInvariant()).ToList();

                    if (references.Count <= 1) continue;

                    var sortedReferences = references.OrderBy(x => x).ToList();

                    var result = references.SequenceEqual(sortedReferences);

                    if (!result && !projFilesWithNonSortedReferences.Contains(proj))
                    {
                        projFilesWithNonSortedReferences.Add(proj);
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

            foreach (var proj in projFiles)
            {
                report.Output($"» {proj}");

                await using var sw = new StringWriter();
                var doc = XDocument.Parse(await System.IO.File.ReadAllTextAsync(proj).ConfigureAwait(false));
                xslt.Transform(doc.CreateNavigator(), null, sw);
                await File.WriteAllTextAsync(proj, sw.ToString()).ConfigureAwait(false);
            }

            return 0;
        }
    }
}
