using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotnetSortAndSyncRefs.Common;

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


    public XmlCentralPackageManagementFile(IFileSystem fileSystem, Reporter reporter)
        : base(fileSystem, reporter)
    {
    }

    public void CreateCentralPackageManagementFile(string path, bool isDryRun)
    {
        FilePath = FileSystem.Path.Combine(path, @"Directory.Packages.props");
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
        return ConstConfig.CentralPackageManagementElementTypes;
    }
}
