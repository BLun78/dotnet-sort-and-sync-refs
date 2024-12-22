using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using DotnetSortAndSyncRefs.Common;

namespace DotnetSortAndSyncRefs.Xml;

internal abstract class XmlBaseFile
{
    protected readonly IFileSystem FileSystem;
    protected readonly Reporter Reporter;
    protected bool IsNoDryRun;

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

    public async Task<int> LoadFileAsync(string filePath, bool isDryRun)
    {
        FilePath = filePath;
        BackupFilePath = $"{filePath}.backup";
        IsNoDryRun = !isDryRun;

        if (FileSystem.File.Exists(FilePath))
        {
            var xmlFile = await FileSystem
                .File
                .ReadAllTextAsync(FilePath)
                .ConfigureAwait(false);
            Document = XDocument.Parse(xmlFile);

            Reporter.Do($"» Backup {FilePath} to {BackupFilePath}");
            if (IsNoDryRun)
            {
                FileSystem.File.Copy(FilePath, BackupFilePath, true);
            }
        }

        return 0;
    }

    public async Task<int> LoadFileReadOnlyAsync(string filePath)
    {
        FilePath = filePath;
        BackupFilePath = $"{filePath}.backup";
        IsNoDryRun = false;

        if (FileSystem.File.Exists(FilePath))
        {
            var xmlFile = await FileSystem
                .File
                .ReadAllTextAsync(FilePath)
                .ConfigureAwait(false);
            Document = XDocument.Parse(xmlFile);
        }

        return 0;
    }

    public async Task SaveAsync()
    {
        if (Document != null && IsNoDryRun)
        {
            await using Stream sw = new FileStream(FilePath, FileMode);
            await sw.FlushAsync().ConfigureAwait(false);
            await Document.SaveAsync(sw, SaveOptions.None, CancellationToken.None);

        }
    }

    protected abstract string GetItemGroupElements();
}