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

namespace DotnetSortAndSyncRefs.Processes
{
    internal class SortReferences : DryRun
    {
        private readonly Reporter _reporter;
        private readonly IFileSystem _fileSystem;

        public SortReferences(
            Reporter reporter,
            IFileSystem fileSystem
            )
        {
            _reporter = reporter;
            _fileSystem = fileSystem;
        }

        public async Task<int> SortIt(IEnumerable<string> projFiles)
        {
            _reporter.Output("Running sort package references ...");
            var result = 5;
            var xslt = GetXslTransform();

            foreach (var projFile in projFiles)
            {
                try
                {
                    await using var sw = new StringWriter();
                    var xml = await _fileSystem.File.ReadAllTextAsync(projFile).ConfigureAwait(false);
                    var doc = XDocument.Parse(xml);
                    xslt.Transform(doc.CreateNavigator(), null, sw);

                    // write file
                    if (IsNoDryRun)
                    {
                        await _fileSystem.File.WriteAllTextAsync(projFile, sw.ToString()).ConfigureAwait(false);
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

        public static XslCompiledTransform GetXslTransform()
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
