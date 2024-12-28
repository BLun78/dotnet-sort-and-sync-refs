using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
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
    [Argument(0, Description =
        "Specifies whether to do a dotnet update. e. g. net481, net9.0")]
    public string FrameworkVersion { get; set; }

    [Argument(1, Description =
        "The path to a .csproj, .vbproj, .fsproj or directory. If a directory is specified, all .csproj, .vbproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
    public override string Path { get; set; }

    private NuGetFramework NuGetFramework => NuGetFramework.Parse(FrameworkVersion);

    public DotnetUpgradeCommand(IServiceProvider serviceProvider)
        : base(serviceProvider, "dotnet-upgrade")
    {
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

                xmlAllElementFile.FixAndGroupItemGroups();
                SetFrameworks(xmlAllElementFile.TargetFrameworks);

                if (IsNoDryRun)
                {
                    await xmlAllElementFile
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

        return 0;
    }

    private void SetFrameworks(XElement? element)
    {
        if (element != null)
        {
            if (element.Name == ConstConfig.TargetFrameworks)
            {
                var frameworks = element.Value.Split(";")
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();
                frameworks.Add(FrameworkVersion);
                frameworks = frameworks
                    .OrderByDescending(x => x)
                    .ToList();
                element.Value = string.Join(";", frameworks);
                return;
            }
            else if (element.Name == ConstConfig.TargetFramework)
            {
                element.Value = FrameworkVersion;
                return;
            }
        }
        throw new InvalidOperationException(
            "No TargetFramework- or TargetFrameworks-Element was found in PropertyGroup.");
    }
}