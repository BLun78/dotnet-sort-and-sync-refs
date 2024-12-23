using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Processes
{
    internal class Inspector : DryRun
    {
        private readonly IServiceProvider _serviceProvider;

        public Inspector(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<List<string>> Inspect(IEnumerable<string> projFiles)
        {
            var projFilesWithNonSortedReferences = new List<string>();
            var reporter = _serviceProvider.GetRequiredService<Reporter>();

            foreach (var projFile in projFiles)
            {

                try
                {
                    var xmlFile = _serviceProvider.GetRequiredService<XmlAllElementFile>();
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
    }
}
