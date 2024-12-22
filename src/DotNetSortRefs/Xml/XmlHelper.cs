using System;
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
using DotnetSortAndSyncRefs.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Xml
{
    internal static class XmlHelper
    {
        public static async Task<List<string>> Inspect(
            this IServiceProvider serviceProvider,
            IEnumerable<string> projFiles,
            bool isDryRun)
        {
            var projFilesWithNonSortedReferences = new List<string>();
            var reporter = serviceProvider.GetRequiredService<Reporter>();

            foreach (var projFile in projFiles)
            {

                try
                {
                    var xmlFile = serviceProvider.GetRequiredService<XmlAllElementFile>();
                    await xmlFile
                        .LoadFileReadOnlyAsync(projFile)
                        .ConfigureAwait(false);

                    foreach (var itemGroup in xmlFile.ItemGroups)
                    {
                        var references = itemGroup
                            .XPathSelectElements(ConstConfig.AllElementTypes)
                            .Select(x => x.Attribute("Include")?.Value.ToLowerInvariant())
                            .ToList();

                        if (references.Count <= 1) continue;

                        var sortedReferences = references
                            .OrderBy(x => x)
                            .ToList();

                        var result = references.SequenceEqual(sortedReferences);

                        if (!result && !projFilesWithNonSortedReferences.Contains(projFile))
                        {
                            reporter.NotOk($"» {projFile}");
                            projFilesWithNonSortedReferences.Add(projFile);
                        }
                        else if (!projFilesWithNonSortedReferences.Contains(projFile))
                        {
                            reporter.Ok($"» {projFile}");
                        }
                    }
                }
                catch (Exception e)
                {
                    reporter.Error($"» {projFile}");
                    reporter.Error(e.Message);
                    return null;
                }
            }

            return projFilesWithNonSortedReferences;
        }

        public static XslCompiledTransform GetXslTransform()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"{nameof(DotnetSortAndSyncRefs)}.Sort.xsl");
            using var reader = XmlReader.Create(stream!);
            var xslt = new XslCompiledTransform();
            xslt.Load(reader);
            return xslt;
        }

        public static async Task<int> SortReferences(this IFileSystem fileSystem, Reporter reporter,
            IEnumerable<string> projFiles, bool dryRun)
        {
            reporter.Output("Running sort package references ...");
            var result = 5;
            var xslt = GetXslTransform();

            foreach (var projFile in projFiles)
            {
                try
                {
                    await using var sw = new StringWriter();
                    var doc = XDocument.Parse(await fileSystem.File.ReadAllTextAsync(projFile).ConfigureAwait(false));
                    xslt.Transform(doc.CreateNavigator(), null, sw);

                    // write file
                    if (!dryRun)
                    {
                        await fileSystem.File.WriteAllTextAsync(projFile, sw.ToString()).ConfigureAwait(false);
                    }

                    reporter.Ok($"» {projFile}");
                    result = 0;
                }
                catch (Exception e)
                {
                    reporter.Error($"» {projFile}");
                    reporter.Error(e.Message);
                }
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
    }
}
