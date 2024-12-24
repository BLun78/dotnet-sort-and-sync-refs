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
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Processes
{
    internal class CentralPackageManagement : DryRun
    {
        private readonly IServiceProvider _serviceProvider;

        public string FilePath { get; set; }
        
        public CentralPackageManagement(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<int> CreateCentralPackageManagementFile(
            List<string> fileProjects)
        {
            var result = ErrorCodes.CreateCentralPackageManagementFailed;
            var error = false;
            var reporter = _serviceProvider.GetRequiredService<Reporter>();
            var centralPackageManagementFile = _serviceProvider.GetRequiredService<XmlCentralPackageManagementFile>();

            centralPackageManagementFile.CreateCentralPackageManagementFile(Program.Path, IsDryRun);
            var itemGroup = centralPackageManagementFile.Document.XPathSelectElements($"//ItemGroup").First();

            var dict = new Dictionary<string, XElement>
            {
                { ConstConfig.WithOutCondition, itemGroup }
            };

            var elementsOfProjectFiles = new List<XElement>();
            foreach (var projFile in fileProjects)
            {
                try
                {
                    result = ErrorCodes.CreateCentralPackageManagementFailed;
                    var xmlProjectFile = _serviceProvider.GetRequiredService<XmlProjectFile>();
                    await xmlProjectFile.LoadFileAsync(projFile, IsDryRun).ConfigureAwait(false);

                    // search for ItemGroup with ProjectElementTypes and for ItemGroup with ProjectElementTypes|Condition
                    var itemGroups = xmlProjectFile.Document
                        .XPathSelectElements($"//ItemGroup[{ConstConfig.ProjectElementTypes}] | //ItemGroup[{ConstConfig.Condition} and {ConstConfig.ProjectElementTypes}]")
                        .ToList();
                    CreateItemGroups(itemGroups, itemGroup, dict);

                   
                    elementsOfProjectFiles.AddRange(itemGroups);

                    var referenceElementsOfProjectFiles = elementsOfProjectFiles.GetReferenceElements();
                    foreach (var element in referenceElementsOfProjectFiles)
                    {
                        var test = new ItemGroup(element.Parent);
                        var condition = GetCondition(element.Parent) ?? ConstConfig.WithOutCondition;

                        if (dict.TryGetValue(condition, out var value))
                        {
                            var newElement = new XElement(element)
                            {
                                Name = ConstConfig.CentralPackageManagementElementTypes
                            };
                            value.Add(newElement);
                            RemoveVersion(element);
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
            FilePath = centralPackageManagementFile.FilePath;
            return result;
        }

        private void CreateItemGroups(IEnumerable<XElement> itemGroups, XElement itemGroup, Dictionary<string, XElement> dict)
        {
            foreach (var element in itemGroups)
            {
                var newItemGroup = CreateItemGroup(element, itemGroup);
                if (newItemGroup != null)
                {
                    itemGroup.AddAfterSelf(newItemGroup);
                    dict.Add(GetCondition(newItemGroup), newItemGroup);
                }
            }
        }

        private XElement CreateItemGroup(XElement inputElement, XElement nodeBeFor)
        {
            var condition = GetCondition(inputElement);

            if (!string.IsNullOrWhiteSpace(condition))
            {
                var element = new XElement("ItemGroup");

                element.SetAttributeValue(ConstConfig.Condition, condition);

                return element;
            }
            return null;
        }

        public string GetCondition(XElement element)
        {
            var condition = element.FirstAttribute;
            if (condition != null &&
                condition.Name == ConstConfig.Condition)
            {
                return condition.Value;
            }
            return null;
        }

        private void RemoveVersion(XElement element)
        {
            var attribute = element.Attribute(ConstConfig.Version);
            attribute?.Remove();
        }
    }
}
