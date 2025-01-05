using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Extensions;
using DotnetSortAndSyncRefs.Xml;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DotnetSortAndSyncRefs.Commands;

[Command("central-package-management", "cpm", "create", "c", Description = "Creates a central package management file and updates all project files.")]
internal class CentralPackageManagementCommand : SortReferences, ICommandBase
{
    [Argument(0, Description =
        "The path to a .csproj, .vbproj, .fsproj or directory. If a directory is specified, all .csproj, .vbproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
    public override string Path { get; set; }

    public CentralPackageManagementCommand(IServiceProvider serviceProvider)
        : base(serviceProvider, "central-package-management")
    {
    }

    public override async Task<int> OnExecute()
    {
        var result = await base.OnExecute().ConfigureAwait(false);
        if (result != ErrorCodes.Ok)
        {
            return result;
        }

        Reporter.Output("Running create central package management for package references ...");

        var error = false;
        var reporter = ServiceProvider.GetRequiredService<Common.IReporter>();
        var centralPackageManagementFile = ServiceProvider.GetRequiredService<XmlCentralPackageManagementFile>();
        var directoryPackagesPropsDoExists = FileSystem.File.Exists(centralPackageManagementFile.GetDirectoryPackagesPropsPath(Path));

        if (directoryPackagesPropsDoExists)
        {
            await centralPackageManagementFile.LoadFileAsync(centralPackageManagementFile.GetDirectoryPackagesPropsPath(Path), IsDryRun);
        }
        else
        {
            centralPackageManagementFile.CreateCentralPackageManagementFile(Path, IsDryRun);
            AllFiles.Add(centralPackageManagementFile.FilePath);
            FileProps.Add(centralPackageManagementFile.FilePath);
        }

        var originItemGroups = centralPackageManagementFile.Document.XPathSelectElements($"//{ConstConfig.ItemGroup}").ToList();
        

        var xmlFilesToSave = new List<XmlBaseFile>();
        foreach (var projFile in FileProjects)
        {
            try
            {

                result = ErrorCodes.CreateCentralPackageManagementFailed;
                var xmlProjectFile = ServiceProvider.GetRequiredService<XmlProjectFile>();
                await xmlProjectFile.LoadFileAsync(projFile, IsDryRun).ConfigureAwait(false);

                // search for ItemGroup with ProjectElementTypes and for ItemGroup with ProjectElementTypes|Condition
                var itemGroups = xmlProjectFile.Document
                    .XPathSelectElements($"//{ConstConfig.ItemGroup}[{ConstConfig.ProjectElementTypesQuery}] | //{ConstConfig.ItemGroup}[{ConstConfig.Condition} and {ConstConfig.ProjectElementTypesQuery}]")
                    .ToList();

                if (directoryPackagesPropsDoExists)
                {
                    xmlProjectFile.UpdateItemGroups(itemGroups, originItemGroups);

                    var removeList = itemGroups.GetPackageReferenceElementsWithVersionSorted();

                    foreach (var element in removeList)
                    {
                        xmlProjectFile.RemoveVersion(element);
                    }
                }
                else
                {
                    xmlProjectFile.CreateItemGroups(itemGroups, originItemGroups);
                    
                    var removeList = itemGroups.GetPackageReferenceElementsWithVersionSorted();

                    foreach (var element in removeList)
                    {
                        xmlProjectFile.RemoveVersion(element);
                    }
                }

                // write file
                if (IsNoDryRun)
                {
                    xmlFilesToSave.Add(xmlProjectFile);
                }

                result = ErrorCodes.Ok;
            }
            catch (Exception e)
            {
                reporter.Error($"» {projFile}");
                reporter.Error(e.Message);
                reporter.Do("An error is thrown, please use the backup files to restore them!");
                error = true;
                break;
            }
        }

        if (error)
        {
            return ErrorCodes.CentralPackageManagementCriticalError;
        }

        // write file
        if (IsNoDryRun)
        {
            foreach (var xmlBaseFile in xmlFilesToSave)
            {
                await xmlBaseFile
                    .SaveAsync()
                    .ConfigureAwait(false);
                reporter.Ok($"» Updated {xmlBaseFile.FilePath}");
            }

            await centralPackageManagementFile
                .SaveAsync()
                .ConfigureAwait(false);
        }
        reporter.Ok($"» Created {centralPackageManagementFile.FilePath}");

        if (!string.IsNullOrWhiteSpace(centralPackageManagementFile.FilePath))
        {
            ProjFilesWithNonSortedReferences.Add(centralPackageManagementFile.FilePath);
        }

        if (result == ErrorCodes.Ok)
        {
            return await base.SortReferencesAsync(result);
        }

        return result;
    }
}