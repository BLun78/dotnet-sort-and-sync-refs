using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Extensions;
using DotnetSortAndSyncRefs.Models;
using DotnetSortAndSyncRefs.Xml;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Commands;

[Command("central-package-management", "cpm", "create", "c", Description = "Creates a central package management file and updates all project files.")]
internal class CentralPackageManagementCommand : SortReferences, ICommandBase
{
    public CentralPackageManagementCommand(IServiceProvider serviceProvider)
        : base(serviceProvider, "central-package-management")
    {
    }

    public override async Task<int> OnExecute()
    {
        var result = ErrorCodes.CreateCentralPackageManagementFailed;
        var error = false;
        var reporter = ServiceProvider.GetRequiredService<Reporter>();
        var centralPackageManagementFile = ServiceProvider.GetRequiredService<XmlCentralPackageManagementFile>();

        centralPackageManagementFile.CreateCentralPackageManagementFile(Path, IsDryRun);
        var itemGroup = centralPackageManagementFile.Document.XPathSelectElements($"//ItemGroup").First();

        var dict = new Dictionary<string, XElement>
            {
                { ConstConfig.WithOutCondition, itemGroup }
            };

        var elementsOfProjectFiles = new List<XElement>();
        foreach (var projFile in FileProjects)
        {
            try
            {
                result = ErrorCodes.CreateCentralPackageManagementFailed;
                var xmlProjectFile = ServiceProvider.GetRequiredService<XmlProjectFile>();
                await xmlProjectFile.LoadFileAsync(projFile, IsDryRun).ConfigureAwait(false);

                xmlProjectFile.FixAndGroupItemGroups();

                // search for ItemGroup with ProjectElementTypes and for ItemGroup with ProjectElementTypes|Condition
                var itemGroups = xmlProjectFile.Document
                    .XPathSelectElements($"//ItemGroup[{ConstConfig.ProjectElementTypes}] | //ItemGroup[{ConstConfig.Condition} and {ConstConfig.ProjectElementTypes}]")
                    .ToList();
                xmlProjectFile.CreateItemGroups(itemGroups, itemGroup, dict);


                elementsOfProjectFiles.AddRange(itemGroups);

                var referenceElementsOfProjectFiles = elementsOfProjectFiles.GetReferenceElements();
                foreach (var element in referenceElementsOfProjectFiles)
                {
                    var condition = xmlProjectFile.GetCondition(element.Parent) ?? ConstConfig.WithOutCondition;

                    if (dict.TryGetValue(condition, out var value))
                    {
                        var newElement = new XElement(element)
                        {
                            Name = ConstConfig.CentralPackageManagementElementTypes
                        };
                        value.Add(newElement);
                        xmlProjectFile.RemoveVersion(element);
                    }
                }

                // write file
                if (IsNoDryRun)
                {
                    await xmlProjectFile
                        .SaveAsync()
                        .ConfigureAwait(false);
                }
                reporter.Ok($"» Updated {projFile}");
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
            return await base.OnExecute();
        }

        return result;
    }
}