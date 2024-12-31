using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using DotnetSortAndSyncRefs.Commands;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Test.Mocks;
using DotnetSortAndSyncRefs.Test.TestContend.CommandBase.TestCommandBaseCtorOk;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DotnetSortAndSyncRefs.Test.Commands
{
    [TestClass]
    public sealed class TestSortReferencesCommand
    {
        [TestMethod]
        public async Task TestSortReferencesOkCommand()
        {
            // arrange
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\solution\Directory.Packages.props", new MockFileData(MockFileStrings.GetDirectoryPackagesPropsUnsorted(), Encoding.UTF8) },
                { @"c:\solution\Test.Dotnet.csproj", new MockFileData(MockFileStrings.GetTestDotnetCsprojUnsorted(), Encoding.UTF8) },
                { @"c:\solution\Test.NetStandard.csproj", new MockFileData(MockFileStrings.GetTestNetStandardCsprojUnsorted(), Encoding.UTF8) }
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem);
            var provider = di.CreateServiceProvider();
            var command = provider.GetRequiredService<SortReferencesCommand>();
            command.Path = path;

            // act
            var result = await command.OnExecute();

            // assert
            Assert.AreEqual(3, command.ProjFilesWithNonSortedReferences.Count); // result of Inspection
            Assert.AreEqual(3, command.AllFiles.Count);
            Assert.AreEqual(2, command.FileProjects.Count);
            Assert.AreEqual(1, command.FileProps.Count);
            Assert.AreEqual(path, command.Path);
            Assert.AreEqual(ErrorCodes.Ok, result);
        }
    }

}
