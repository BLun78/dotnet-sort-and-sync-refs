using DotnetSortAndSyncRefs.Common;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
using System.Xml.Xsl;
using System.Xml;
using DotnetSortAndSyncRefs.Xml;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace DotnetSortAndSyncRefs.Processes
{
    internal class SortReferences : DryRun
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Reporter _reporter;

        public SortReferences(
            IServiceProvider serviceProvider,
            Reporter reporter
            )
        {
            _serviceProvider = serviceProvider;
            _reporter = reporter;
        }

        public async Task<int> SortIt(IEnumerable<string> projFiles, CancellationToken cancellationToken = default)
        {
            _reporter.Output("Running sort package references ...");
            var result = 5;
            var xslt = GetXslTransform();

            foreach (var projFile in projFiles)
            {
                try
                {
                    await using var sw = new StringWriter();
                    var xmlAllElementFile = _serviceProvider.GetRequiredService<XmlAllElementFile>();
                    await xmlAllElementFile
                        .LoadFileAsync(projFile, IsDryRun, cancellationToken)
                        .ConfigureAwait(false);
                    
                    xslt.Transform(xmlAllElementFile.Document.CreateNavigator(), null, sw);

                    // write file
                    if (IsNoDryRun)
                    {
                        await xmlAllElementFile
                            .SaveAsync(sw, cancellationToken)
                            .ConfigureAwait(false);
                    }

                    _reporter.Ok($"» {projFile}");
                    result = ErrorCodes.Ok;
                }
                catch (Exception e)
                {
                    _reporter.Error($"» {projFile}");
                    _reporter.Error(e.Message);
                }
            }

            return result;
        }

        private static XslCompiledTransform GetXslTransform()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"{nameof(DotnetSortAndSyncRefs)}.Sort.xsl");
            using var reader = XmlReader.Create(stream!);
            var xslt = new XslCompiledTransform();
            xslt.Load(reader);
            return xslt;
        }
    }
}
