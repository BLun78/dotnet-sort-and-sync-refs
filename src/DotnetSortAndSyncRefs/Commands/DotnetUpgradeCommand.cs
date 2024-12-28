using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Services;
using DotnetSortAndSyncRefs.Xml;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Frameworks;

namespace DotnetSortAndSyncRefs.Commands;

[Command("dotnet-upgrade", "upgrade", "ug",
    Description = "Dotnet Upgrade in all project files, can handle Multi-Framework Projects.")]
internal partial class DotnetUpgradeCommand : CommandBase, ICommandBase
{
    private readonly NuGetService _nuGetService;

    [Argument(0, Description =
        "Specifies whether to do a dotnet update. e. g. net481, net9.0")]
    public string FrameworkVersion { get; set; }

    [Argument(1, Description =
        "The path to a .csproj, .vbproj, .fsproj or directory. If a directory is specified, all .csproj, .vbproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
    public override string Path { get; set; }

    private NuGetFramework NuGetFramework => NuGetFramework.Parse(FrameworkVersion);

    [GeneratedRegex(@"net\d{1,2}\.\d$|netcoreapp\d{1,2}\.\d$", RegexOptions.IgnoreCase)]
    private static partial Regex DotnetCoreRegex();

    [GeneratedRegex(@"netstandard\d\.\d$", RegexOptions.IgnoreCase)]
    private static partial Regex NetStandardRegex();

    [GeneratedRegex(@"net\d{2,3}$", RegexOptions.IgnoreCase)]
    private static partial Regex NetFrameworkRegex();

    public DotnetUpgradeCommand(IServiceProvider serviceProvider,
        NuGetService nuGetService)
        : base(serviceProvider, "dotnet-upgrade")
    {
        _nuGetService = nuGetService;
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
                DuplicateItemGroupsWithFrameworkCondition(xmlAllElementFile);

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

    private void DuplicateItemGroupsWithFrameworkCondition(XmlBaseFile xmlBaseFile)
    {
        var r = GetNewItemGroupWithConditions(xmlBaseFile, FrameworkVersion);
     


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

    /// <summary>
    /// TODO: Support all frameworks
    /// https://learn.microsoft.com/en-us/dotnet/standard/frameworks
    /// </summary>
    /// <param name="itemGroupsConditionGrouped"></param>
    /// <param name="frameworkVersion"></param>
    /// <returns></returns>
    public XElement GetNewItemGroupWithConditions(
        XmlBaseFile itemGroupsConditionGrouped,
        string frameworkVersion)
    {
        var itemGroupsWithCondition =
            itemGroupsConditionGrouped
                .ItemGroupsConditionGrouped
                .Where(x => x.Key != ConstConfig.WithOutCondition)
                .ToList();
        var dotnetCoreRegex = DotnetCoreRegex();
        var netStandardRegex = NetStandardRegex();
        var netFrameworkRegex = NetFrameworkRegex();

        if (netStandardRegex.IsMatch(frameworkVersion))
        {
            foreach (var item in itemGroupsWithCondition
                         .Where(x => netStandardRegex.IsMatch(x.Key)
                             ).OrderByDescending(x => x.Key)
                         .ToList()
                     )
            {
                var element = item.Value.FirstOrDefault();

                var newItem = itemGroupsConditionGrouped.CloneItemGroupWithNewFrameworkCondition(element, frameworkVersion);
                _nuGetService.UpdateReferences(newItem, frameworkVersion);

                return newItem;

            }
        }
        else if (dotnetCoreRegex.IsMatch(frameworkVersion))
        {
            foreach (var item in itemGroupsWithCondition
                         .Where(x => dotnetCoreRegex.IsMatch(x.Key)
                         ).OrderByDescending(x => x.Key)
                         .ToList()
                    )
            {
                var element = item.Value.FirstOrDefault();

                var newItem = itemGroupsConditionGrouped.CloneItemGroupWithNewFrameworkCondition(element, frameworkVersion);
                _nuGetService.UpdateReferences(newItem, frameworkVersion);

                return newItem;
            }
        }
        else if (netFrameworkRegex.IsMatch(frameworkVersion))
        {
            foreach (var item in itemGroupsWithCondition
                         .Where(x => netFrameworkRegex.IsMatch(x.Key)
                         ).OrderByDescending(x => x.Key.PadRight(6, '0'))
                         .ToList()
                    )
            {
                var element = item.Value.FirstOrDefault();

                var newItem = itemGroupsConditionGrouped.CloneItemGroupWithNewFrameworkCondition(element, frameworkVersion);
                _nuGetService.UpdateReferences(newItem, frameworkVersion);

                return newItem;

            }
        }
        else
        {
            throw new NotSupportedException($"Framework {frameworkVersion} is not supported.");
        }

        return null;
    }
}