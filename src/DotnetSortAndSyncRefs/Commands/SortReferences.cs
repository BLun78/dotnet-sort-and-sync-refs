using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Commands;

internal abstract class SortReferences : CommandBase, ICommandBase
{
    protected SortReferences(
        IServiceProvider serviceProvider, 
        string commandStartMessage
    ) : base(serviceProvider, commandStartMessage)
    {
    }

    public override async Task<int> OnExecute()
    {
        var result = await base.OnExecute().ConfigureAwait(false);
        if (result != ErrorCodes.Ok)
        {
            return result;
        }

        return await SortReferencesAsync(result);
    }

    public async Task<int> SortReferencesAsync(int result )
    {
        Reporter.Output("Running sort package references ...");
      
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