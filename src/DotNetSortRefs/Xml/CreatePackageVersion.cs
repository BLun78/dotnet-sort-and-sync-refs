using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using DotnetSortAndSyncRefs.Common;

namespace DotnetSortAndSyncRefs.Xml
{
    internal static class CreatePackageVersion
    {
        private const string InitialFile =
            """
            <Project>
              <PropertyGroup>
                <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
              </PropertyGroup>
              <ItemGroup />
            </Project>
            """;

        public static async Task<int> CreatePackageVersions(this IFileSystem fileSystem,
            Reporter reporter,
            List<string> fileProjects,
            string path, 
            bool dryRun)
        {
            var result = 3;
            var error = false;

            var directoryPackagesPropsFilePath = fileSystem.Path.Combine(path, @"Directory.Packages.props");
            var directoryPackagesPropsFileBackupPath = $"{directoryPackagesPropsFilePath}.backup";
            var directoryPackagesPropsFileMode = FileMode.CreateNew;
            var doc = XDocument.Parse(InitialFile);
            var itemGroup = doc.XPathSelectElements($"//ItemGroup").First();

            var dict = new Dictionary<string, XElement>
            {
                { ConstConfig.WithOutCondition, itemGroup }
            };

            if (fileSystem.File.Exists(directoryPackagesPropsFilePath))
            {
                reporter.Do($"» Backup {directoryPackagesPropsFilePath} to {directoryPackagesPropsFileBackupPath}");
                if (!dryRun)
                {
                    fileSystem.File.Copy(directoryPackagesPropsFilePath, directoryPackagesPropsFileBackupPath, true);
                }
                directoryPackagesPropsFileMode = FileMode.Truncate;
            }

            var elementsOfProjectFiles = new List<XElement>();
            foreach (var projFile in fileProjects)
            {
                try
                {
                    result = 3;
                    var backupFilePath = $"{projFile}.backup";
                    reporter.Do($"» Backup {projFile} to {backupFilePath}");
                    if (!dryRun)
                    {
                        fileSystem.File.Copy(projFile, backupFilePath, true);
                    }

                    var docProjFile = XDocument.Parse(await fileSystem.File.ReadAllTextAsync(projFile).ConfigureAwait(false));

                    // search for ItemGroup with ProjectElementTypes and for ItemGroup with ProjectElementTypes|Condition
                    var itemGroups = docProjFile
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
                                Name = ConstConfig.PropsElementTypes
                            };
                            value.Add(newElement);
                            element.RemoveVersion();
                        }
                    }

                    // write file
                    if (!dryRun)
                    {
                        await XmlHelper.SaveXDocument(fileSystem, projFile, docProjFile, FileMode.Truncate);
                    }
                    reporter.Ok($"» Updated {projFile}");
                    result = 0;
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
                return -5;
            }

            // write file
            if (!dryRun)
            {
                await XmlHelper.SaveXDocument(fileSystem, directoryPackagesPropsFilePath, doc, directoryPackagesPropsFileMode);
            }
            reporter.Ok($"» Created {directoryPackagesPropsFilePath}");

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
