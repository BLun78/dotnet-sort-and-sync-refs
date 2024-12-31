using System.IO;
using System.IO.Abstractions;
using System.Linq;
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


    public XmlCentralPackageManagementFile(IFileSystem fileSystem, IReporter reporter)
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

    public void FixDoubleEntriesInItemGroup()
    {
        foreach (var item in ItemGroups)
        {
            var sortedReferences = item
                .Elements(ConstConfig.CentralPackageManagementElementTypes)
                .Where(x => x.Attribute(ConstConfig.Version) != null)
                .OrderByDescending(x => (string)x.Attribute(ConstConfig.Include))
                .ThenBy(x => (string)x.Attribute(ConstConfig.Version))
                .ToList();
            
            var groupedReferences = sortedReferences.GroupBy(x => new
            {
                Include = (string)x.Attribute(ConstConfig.Include),
                Version = (string)x.Attribute(ConstConfig.Version),

            }).ToList();

            if (sortedReferences.Count != groupedReferences.Count)
            {
                foreach (var xElement in sortedReferences)
                {
                    xElement.Remove();
                }
                foreach (var reference in groupedReferences)
                {
                    var newElement = new XElement(ConstConfig.CentralPackageManagementElementTypes);

                    newElement.SetAttributeValue(ConstConfig.Include, reference.Key.Include);
                    newElement.SetAttributeValue(ConstConfig.Version, reference.Key.Version);

                    item.AddFirst(newElement);
                }
            }

        }
    }
}
