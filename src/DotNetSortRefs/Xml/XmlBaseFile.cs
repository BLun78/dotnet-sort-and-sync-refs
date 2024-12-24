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

    public async Task<int> LoadFileAsync(string filePath, bool isDryRun, CancellationToken cancellationToken = default)
    {
        FilePath = filePath;
        BackupFilePath = $"{filePath}.backup";
        IsNoDryRun = !isDryRun;

        if (FileSystem.File.Exists(FilePath))
        {
            var xmlFile = await FileSystem
                .File
                .ReadAllTextAsync(FilePath, cancellationToken)
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

    public async Task<int> LoadFileReadOnlyAsync(string filePath, CancellationToken cancellationToken = default)
    {
        FilePath = filePath;
        BackupFilePath = $"{filePath}.backup";
        IsNoDryRun = false;

        if (FileSystem.File.Exists(FilePath))
        {
            var xmlFile = await FileSystem
                .File
                .ReadAllTextAsync(FilePath, cancellationToken)
                .ConfigureAwait(false);
            Document = XDocument.Parse(xmlFile);
        }

        return 0;
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

    protected abstract string GetItemGroupElements();
}