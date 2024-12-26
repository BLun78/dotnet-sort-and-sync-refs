using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Models;

namespace DotnetSortAndSyncRefs.Xml;

internal abstract class XmlBaseFile
{
    protected readonly IFileSystem FileSystem;
    protected readonly Reporter Reporter;
    protected bool IsNoDryRun;
    protected bool DoBackup;


    protected XmlBaseFile(
        IFileSystem fileSystem,
        Reporter reporter)
    {
        FileSystem = fileSystem;
        Reporter = reporter;
    }

    public string FilePath { get; protected set; }

    public string BackupFilePath { get; protected set; }

    public FileMode FileMode { get; protected set; } = FileMode.Truncate;

    public XDocument Document { get; protected set; }

    public IEnumerable<XElement> ItemGroups => Document?.XPathSelectElements($"//ItemGroup[{GetItemGroupElements()}]");

    public IEnumerable<ItemGroup> ParsedItemGroups
    {
        get
        {
            foreach (var element in ItemGroups)
            {
                yield return new ItemGroup(element);
            }
        }
    }

    public IDictionary<string, List<XElement>> ItemGroupsConditionGrouped
    {
        get
        {
            var dict = new Dictionary<string, List<XElement>>();
            foreach (var itemGroup in ItemGroups)
            {
                var condition = GetCondition(itemGroup);
                if (!string.IsNullOrWhiteSpace(condition))
                {
                    if (dict.ContainsKey(condition))
                    {
                        dict[condition].Add(itemGroup);
                    }
                    else
                    {
                        dict.Add(condition, new List<XElement>() { itemGroup });
                    }
                }
                else
                {
                    if (dict.ContainsKey(ConstConfig.WithOutCondition))
                    {
                        dict[ConstConfig.WithOutCondition].Add(itemGroup);
                    }
                    else
                    {
                        dict.Add(ConstConfig.WithOutCondition, new List<XElement>() { itemGroup });
                    }
                }
            }
            return dict;
        }
    }

    public Task<int> LoadFileAsync(string filePath, bool isDryRun, CancellationToken cancellationToken = default)
    {
        return LoadFileAsync(filePath, isDryRun, true, cancellationToken);
    }

    public async Task<int> LoadFileAsync(string filePath, bool isDryRun, bool doBackup, CancellationToken cancellationToken = default)
    {
        FilePath = filePath;
        BackupFilePath = $"{filePath}.backup";
        IsNoDryRun = !isDryRun;
        DoBackup = doBackup;

        if (FileSystem.File.Exists(FilePath))
        {
            var xmlFile = await FileSystem
                .File
                .ReadAllTextAsync(FilePath, cancellationToken)
                .ConfigureAwait(false);
            Document = XDocument.Parse(xmlFile);

            if (DoBackup)
            {
                Reporter.Do($"» Backup {FilePath} to {BackupFilePath}");
                if (IsNoDryRun)
                {
                    FileSystem.File.Copy(FilePath, BackupFilePath, true);
                }
            }
        }

        return ErrorCodes.Ok;
    }

    public async Task<int> LoadFileReadOnlyAsync(string filePath, CancellationToken cancellationToken = default)
    {
        FilePath = filePath;
        BackupFilePath = $"{filePath}.backup";
        IsNoDryRun = false;
        DoBackup = false;

        if (FileSystem.File.Exists(FilePath))
        {
            var xmlFile = await FileSystem
                .File
                .ReadAllTextAsync(FilePath, cancellationToken)
                .ConfigureAwait(false);
            Document = XDocument.Parse(xmlFile);
        }

        return ErrorCodes.Ok;
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (Document != null && IsNoDryRun)
        {
            await using Stream fileStream = new FileStream(FilePath, FileMode);

            await fileStream
                .FlushAsync(cancellationToken)
                .ConfigureAwait(false);

            await Document
                .SaveAsync(fileStream, SaveOptions.DisableFormatting, cancellationToken)
                .ConfigureAwait(false);

        }
    }

    public async Task SaveAsync(StringWriter stringWriter, CancellationToken cancellationToken = default)
    {
        await stringWriter
            .FlushAsync()
            .ConfigureAwait(false);

        await FileSystem
            .File
            .WriteAllTextAsync(FilePath, stringWriter.ToString(), cancellationToken)
            .ConfigureAwait(false);

    }

    public void FixAndGroupItemGroups()
    {

        foreach (var item in ItemGroupsConditionGrouped)
        {
            var values = item.Value;
            var firstItemGroup = values.FirstOrDefault();
            foreach (var group in values)
            {
                if (firstItemGroup == group) continue;
                var firstItemGroupLaseReference = firstItemGroup.LastNode;
                var node = group.FirstNode;
                while (node != null)
                {
                    firstItemGroupLaseReference.AddAfterSelf(node);

                    node = node.NextNode;
                }
                group.Remove();
            }
        }
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

    public void RemoveVersion(XElement element)
    {
        var attribute = element.Attribute(ConstConfig.Version);
        attribute?.Remove();
    }

    public void CreateItemGroups(IEnumerable<XElement> itemGroups, XElement itemGroup, Dictionary<string, XElement> dict)
    {
        foreach (var element in itemGroups)
        {
            var newItemGroup = CreateItemGroup(element);
            if (newItemGroup != null)
            {
                itemGroup.AddAfterSelf(newItemGroup);
                dict.Add(GetCondition(newItemGroup), newItemGroup);
            }
        }
    }

    public XElement CreateItemGroup(XElement inputElement)
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

    protected abstract string GetItemGroupElements();
}