using System;
using System.Threading.Tasks;
using DotnetSortAndSyncRefs.Common;
using McMaster.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace DotnetSortAndSyncRefs.Commands;

[Command("dotnet-upgrade", "upgrade", "ug", Description = "Dotnet Upgrade in all project files, can handle Multi-Framework Projects.")]
internal class DotnetUpgradeCommand : CommandBase, ICommandBase
{
    [Option(CommandOptionType.SingleValue, Description = "Specifies whether to do a dotnet update. e. g. net481, net9.0",
        ShortName = "framework", LongName = "framework-version")]
    public string FrameworkVersion { get; set; }

    private NuGetFramework _framework;

    public DotnetUpgradeCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _framework = NuGetFramework.Parse(FrameworkVersion);
    }

    public override Task<int> OnExecute()
    {
        
        throw new NotImplementedException();
    }
}