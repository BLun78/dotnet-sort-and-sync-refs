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
            var pathOfInputFile1 = @"c:\solution\Test.Dotnet.csproj";
            var pathOfInputFile2 = @"c:\solution\Test.NetStandard.csproj";
            var mockOption = new MockFileSystemOptions()
            {
                CurrentDirectory = pathOfExecution,
                CreateDefaultTempDir = false
            };
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { pathOfInputFile1, new MockFileData(MockFileStrings.GetTestDotnetCsprojUnsorted(), Encoding.UTF8) },
                { pathOfInputFile2, new MockFileData(MockFileStrings.GetTestNetStandardCsprojUnsorted(), Encoding.UTF8) }
            }, mockOption);

            var di = new DependencyInjectionMock(fileSystem);
            var provider = di.CreateServiceProvider();
            var reporter = provider.GetRequiredService<IReporter>();
            var command = provider.GetRequiredService<CentralPackageManagementCommand>();
            var xmlInputFile1 = provider.GetRequiredService<XmlAllElementFile>();
            await xmlInputFile1.LoadFileAsync(pathOfInputFile1, false, false, false);
            var xmlInputFile2 = provider.GetRequiredService<XmlAllElementFile>();
            await xmlInputFile2.LoadFileAsync(pathOfInputFile1, false, false, false);
            var xmlResultFileResult = provider.GetRequiredService<XmlAllElementFile>();
            command.Path = path;
            reporter.Output("Input File 1:");
            reporter.Output(xmlInputFile1.ToString());
            reporter.Output("Input File 2:");
            reporter.Output(xmlInputFile2.ToString());

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

            await xmlResultFileResult.LoadFileAsync(pathOfResultFile, false, false, false);
            Assert.AreEqual(3, xmlResultFileResult.ItemGroups.ToList().Count);
            reporter.Output("Result File:");
            reporter.Output(xmlResultFileResult.ToString());
        }
    }
}
