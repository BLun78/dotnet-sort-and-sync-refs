using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using DotnetSortAndSyncRefs.Commands;
using DotnetSortAndSyncRefs.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Common;

internal class SortReferences : CommandBase, ICommandBase
{
    public SortReferences(
        IServiceProvider serviceProvider
    ) : base(serviceProvider)
    {
    }

    public override async Task<int> OnExecute()
    {
        Reporter.Output("Running sort package references ...");
        var result = 5;
        var xslt = GetXslTransform();

        foreach (var projFile in AllFiles)
        {
            try
            {
                await using var sw = new StringWriter();
                var xmlAllElementFile = ServiceProvider.GetRequiredService<XmlAllElementFile>();
                await xmlAllElementFile
                    .LoadFileAsync(projFile, IsDryRun, false)
                    .ConfigureAwait(false);

                xslt.Transform(xmlAllElementFile.Document.CreateNavigator(), null, sw);

                // write file
                if (IsNoDryRun)
                {
                    await xmlAllElementFile
                        .SaveAsync(sw)
                        .ConfigureAwait(false);
                }

                Reporter.Ok($"» {projFile}");
                result = ErrorCodes.Ok;
            }
            catch (Exception e)
            {
                Reporter.Error($"» {projFile}");
                Reporter.Error(e.Message);
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