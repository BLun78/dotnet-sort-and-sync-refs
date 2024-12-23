using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using DotnetSortAndSyncRefs.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Xml
{
    internal static class CentralPackageManagement
    {
        public static async Task<int> CreateCentralPackageManagementFile(
            this IServiceProvider serviceProvider,
            List<string> fileProjects,
            string path,
            bool dryRun)
        {
            var result = ErrorCodes.CreateCentralPackageManagementFailed;
            var error = false;
            var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
            var reporter = serviceProvider.GetRequiredService<Reporter>();
            var centralPackageManagementFile = serviceProvider.GetRequiredService<XmlCentralPackageManagementFile>();

            centralPackageManagementFile.CreateCentralPackageManagementFile(path, dryRun);
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
                    var xmlProjectFile = serviceProvider.GetRequiredService<XmlProjectFile>();
                    await xmlProjectFile.LoadFileAsync(projFile, dryRun).ConfigureAwait(false);

                    // search for ItemGroup with ProjectElementTypes and for ItemGroup with ProjectElementTypes|Condition
                    var itemGroups = xmlProjectFile.Document
                        .XPathSelectElements($"//ItemGroup[{ConstConfig.ProjectElementTypes}] | //ItemGroup[{ConstConfig.Condition} and {ConstConfig.ProjectElementTypes}]")
                        .ToList();
                    CreateItemGroups(itemGroups, itemGroup, dict);

                    elementsOfProjectFiles.AddRange(itemGroups);

                    var referenceElementsOfProjectFiles = elementsOfProjectFiles.GetReferenceElements();
                    foreach (var element in referenceElementsOfProjectFiles)
                    {
                        var condition = element.Parent.GetCondition() ?? ConstConfig.WithOutCondition;

                        if (dict.TryGetValue(condition, out var value))
                        {
                            var newElement = new XElement(element)
                            {
                                Name = ConstConfig.CentralPackageManagementElementTypes
                            };
                            value.Add(newElement);
                            element.RemoveVersion();
                        }
                    }

                    // write file
                    if (!dryRun)
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
            if (!dryRun)
            {
                await centralPackageManagementFile
                    .SaveAsync()
                    .ConfigureAwait(false);
            }
            reporter.Ok($"» Created {centralPackageManagementFile.FilePath}");

            return result;
        }

        private static void CreateItemGroups(IEnumerable<XElement> itemGroups, XElement itemGroup, Dictionary<string, XElement> dict)
        {
            foreach (var element in itemGroups)
            {
                var newItemGroup = element.CreateItemGroup(itemGroup);
                if (newItemGroup != null)
                {
                    itemGroup.AddAfterSelf(newItemGroup);
                    dict.Add(newItemGroup.GetCondition(), newItemGroup);
                }
            }
        }

        private static XElement? CreateItemGroup(this XElement inputElement, XElement nodeBeFor)
        {
            var condition = inputElement.GetCondition();

            if (!string.IsNullOrWhiteSpace(condition))
            {
                var element = new XElement("ItemGroup");

                element.SetAttributeValue(ConstConfig.Condition, condition);

                return element;
            }
            return null;
        }

        public static string GetCondition(this XElement element)
        {
            var condition = element.FirstAttribute;
            if (condition != null &&
                condition.Name == ConstConfig.Condition)
            {
                return condition.Value;
            }
            return null;
        }

        private static void RemoveVersion(this XElement element)
        {
            var attribute = element.Attribute(ConstConfig.Version);
            attribute?.Remove();
        }
    }
}
