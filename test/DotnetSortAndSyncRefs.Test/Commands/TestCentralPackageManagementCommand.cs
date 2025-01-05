using DotnetSortAndSyncRefs.Commands;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Test.Mocks;
using DotnetSortAndSyncRefs.Test.TestContend.CommandBase.TestCommandBaseCtorOk;
using DotnetSortAndSyncRefs.Xml;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace DotnetSortAndSyncRefs.Test.Commands
{
    [TestClass]
    public class TestCentralPackageManagementCommand
    {
        [TestMethod]
        public async Task Test_CentralPackageManagementCommand_Ok()
        {
            // arrange
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var pathOfResultFile = @"c:\solution\Directory.Packages.props";
            var mockOption = new MockFileSystemOptions()
            {
                CurrentDirectory = pathOfExecution,
                CreateDefaultTempDir = false
            };
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                //{ @"c:\solution\Directory.Packages.props", new MockFileData(MockFileStrings.GetDirectoryPackagesPropsUnsorted(), Encoding.UTF8) },
                { @"c:\solution\Test.Dotnet.csproj", new MockFileData(MockFileStrings.GetTestDotnetCsprojUnsorted(), Encoding.UTF8) },
                { @"c:\solution\Test.NetStandard.csproj", new MockFileData(MockFileStrings.GetTestNetStandardCsprojUnsorted(), Encoding.UTF8) }
            }, mockOption);

            var di = new DependencyInjectionMock(fileSystem);
            var provider = di.CreateServiceProvider();
            var command = provider.GetRequiredService<CentralPackageManagementCommand>();
            var xmlResultFile = provider.GetRequiredService<XmlCentralPackageManagementFile>();
            command.Path = path;

            // act
            var result = await command.OnExecute();

            // assert
            Assert.AreEqual(3, command.ProjFilesWithNonSortedReferences.Count); // result of Inspection
            Assert.AreEqual(2, command.AllFiles.Count);
            Assert.AreEqual(2, command.FileProjects.Count);
            Assert.AreEqual(0, command.FileProps.Count);
            Assert.AreEqual(path, command.Path);
            Assert.AreEqual(ErrorCodes.Ok, result);
            Assert.IsTrue(fileSystem.FileExists(pathOfResultFile));

            await xmlResultFile.LoadFileAsync(pathOfResultFile, false, false, false);
            Assert.AreEqual(3, xmlResultFile.ItemGroups.ToList().Count);
        }
    }
}
