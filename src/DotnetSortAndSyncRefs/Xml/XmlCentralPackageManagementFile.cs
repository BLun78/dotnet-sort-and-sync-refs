using DotnetSortAndSyncRefs.Common;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DotnetSortAndSyncRefs.Xml;

internal class XmlCentralPackageManagementFile : XmlBaseFile
{
    private const string InitialFile =
        $$"""
        <Project>
          <{{ConstConfig.PropertyGroup}}>
            <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
          </{{ConstConfig.PropertyGroup}}>
          <{{ConstConfig.ItemGroup}} />
        </Project>
        """;
    
    public XmlCentralPackageManagementFile(IFileSystem fileSystem, IReporter reporter)
        : base(fileSystem, reporter)
    {
    }

    public string GetDirectoryPackagesPropsPath(string path)
    {
        return FileSystem.Path.Combine(path, @"Directory.Packages.props");
    }

    public void CreateCentralPackageManagementFile(string path, bool isDryRun)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            FilePath = GetDirectoryPackagesPropsPath(path);
        }
        BackupFilePath = $"{FilePath}.backup";
        IsNoDryRun = !isDryRun;

        Document = XDocument.Parse(InitialFile);
        FileMode = FileMode.CreateNew;

        if (FileSystem.File.Exists(FilePath))
        {
            if (DoBackup)
            {
                Reporter.Do($"» Backup {FilePath} to {BackupFilePath}");
                if (IsNoDryRun)
                {
                    FileSystem.File.Copy(FilePath, BackupFilePath, true);
                }
            }
            FileMode = FileMode.Truncate;
        }
    }

    protected override string GetItemGroupElements()
    {
        return ConstConfig.PackageVersion;
    }

    public override Task SaveAsync(CancellationToken cancellationToken = default)
    {

        FixAndGroupItemGroups();
        FixDoubleEntriesInItemGroup();

        return base.SaveAsync(cancellationToken);
    }
}
