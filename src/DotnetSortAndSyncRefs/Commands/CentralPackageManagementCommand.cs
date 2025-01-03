﻿using System;
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
    [Argument(0, Description =
        "The path to a .csproj, .vbproj, .fsproj or directory. If a directory is specified, all .csproj, .vbproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
    public override string Path { get; set; }

    public CentralPackageManagementCommand(IServiceProvider serviceProvider)
        : base(serviceProvider, "central-package-management")
    {
    }

    public override async Task<int> OnExecute()
    {
        var result = ErrorCodes.CreateCentralPackageManagementFailed;
        var error = false;
        var reporter = ServiceProvider.GetRequiredService<Common.IReporter>();
        var centralPackageManagementFile = ServiceProvider.GetRequiredService<XmlCentralPackageManagementFile>();

        centralPackageManagementFile.CreateCentralPackageManagementFile(Path, IsDryRun);
        var itemGroup = centralPackageManagementFile.Document.XPathSelectElements($"//{ConstConfig.ItemGroup}").First();

        var dict = new Dictionary<string, XElement>
            {
                { ConstConfig.WithOutCondition, itemGroup }
            };

        var elementsOfProjectFiles = new List<XElement>();
        var xmlFilesToSave = new List<XmlBaseFile>();
        foreach (var projFile in FileProjects)
        {
            try
            {
                elementsOfProjectFiles.Clear();

                result = ErrorCodes.CreateCentralPackageManagementFailed;
                var xmlProjectFile = ServiceProvider.GetRequiredService<XmlProjectFile>();
                await xmlProjectFile.LoadFileAsync(projFile, IsDryRun).ConfigureAwait(false);

                xmlProjectFile.FixAndGroupItemGroups();

                // search for ItemGroup with ProjectElementTypes and for ItemGroup with ProjectElementTypes|Condition
                var itemGroups = xmlProjectFile.Document
                    .XPathSelectElements($"//{ConstConfig.ItemGroup}[{ConstConfig.ProjectElementTypesQuery}] | //{ConstConfig.ItemGroup}[{ConstConfig.Condition} and {ConstConfig.ProjectElementTypesQuery}]")
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
            centralPackageManagementFile.FixAndGroupItemGroups();
            centralPackageManagementFile.FixDoubleEntriesInItemGroup();

           
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
            return await base.OnExecute();
        }

        return result;
    }
}