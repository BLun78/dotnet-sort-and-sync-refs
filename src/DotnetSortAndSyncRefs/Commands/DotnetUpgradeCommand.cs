using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Xml;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Frameworks;

namespace DotnetSortAndSyncRefs.Commands;

[Command("dotnet-upgrade", "upgrade", "ug", 
    Description = "Dotnet Upgrade in all project files, can handle Multi-Framework Projects.")]
internal class DotnetUpgradeCommand : CommandBase, ICommandBase
{
    [Option(CommandOptionType.SingleValue, Description = "Specifies whether to do a dotnet update. e. g. net481, net9.0",
        ShortName = "framework", LongName = "framework-version")]
    public string FrameworkVersion { get; set; }

    private NuGetFramework _framework;

    public DotnetUpgradeCommand(IServiceProvider serviceProvider) 
        : base(serviceProvider, "dotnet-upgrade")
    {
        //_framework = NuGetFramework.Parse(FrameworkVersion);
    }

    public override async Task<int> OnExecute()
    {
        var xmlCentralPackageManagementFile = ServiceProvider.GetRequiredService<XmlCentralPackageManagementFile>();
        var cpm = FileProps.First(x => x.Contains(CentralPackageManagementFile));
        await xmlCentralPackageManagementFile
            .LoadFileAsync(cpm, IsDryRun)
            .ConfigureAwait(false);

        foreach (var projFile in FileProjects)
        {
            try
            {
                var xmlAllElementFile = ServiceProvider.GetRequiredService<XmlAllElementFile>();
                await xmlAllElementFile
                    .LoadFileAsync(projFile, IsDryRun, false)
                    .ConfigureAwait(false);

               
                

                if (IsNoDryRun)
                {
                    xmlAllElementFile
                        .SaveAsync()
                        .ConfigureAwait(false);
                }
                Reporter.Ok($"» {projFile}");
            }
            catch (Exception e)
            {
                Reporter.Error($"» {projFile}");
                Reporter.Error(e.Message);
            }

        }


        throw new NotImplementedException();
    }
}