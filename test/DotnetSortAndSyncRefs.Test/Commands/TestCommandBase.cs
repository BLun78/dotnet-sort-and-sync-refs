using DotnetSortAndSyncRefs.Test.Mocks;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Test.TestContend.CommandBase.TestCommandBaseCtorOk;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSortAndSyncRefs.Test.Commands
{
    [TestClass]
    public class TestCommandBase
    {
        [TestMethod]
        public async Task TestCommandBaseOkAsync()
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
            var command = provider.GetRequiredService<CommandBaseTest>();
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

        [TestMethod]
        public async Task TestCommandBaseOkSortedAsync()
        {
            // arrange
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\solution\Directory.Packages.props", new MockFileData(MockFileStrings.GetDirectoryPackagesPropsUnsorted(), Encoding.UTF8) },
                { @"c:\solution\Test.Dotnet.csproj", new MockFileData(MockFileStrings.GetTestDotnetCsprojSorted(), Encoding.UTF8) },
                { @"c:\solution\Test.NetStandard.csproj", new MockFileData(MockFileStrings.GetTestNetStandardCsprojUnsorted(), Encoding.UTF8) }
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem);
            di.ServiceCollection.AddSingleton<CommandBaseTest>();
            var provider = di.CreateServiceProvider();
            var command = provider.GetRequiredService<CommandBaseTest>();
            command.Path = path;

            // act
            var result = await command.OnExecute();

            // assert
            Assert.AreEqual(2, command.ProjFilesWithNonSortedReferences.Count); // result of Inspection
            Assert.AreEqual(3, command.AllFiles.Count);
            Assert.AreEqual(2, command.FileProjects.Count);
            Assert.AreEqual(1, command.FileProps.Count);
            Assert.AreEqual(path, command.Path);
            Assert.AreEqual(ErrorCodes.Ok, result);
        }

        [TestMethod]
        public async Task TestCommandBaseFileDoNotExistsAsync()
        {
            // arrange
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\solution\Directory.Packages.props1", new MockFileData(MockFileStrings.GetDirectoryPackagesPropsUnsorted(), Encoding.UTF8) },
                { @"c:\solution\Test.Dotnet.csproj2", new MockFileData(MockFileStrings.GetTestDotnetCsprojUnsorted(), Encoding.UTF8) },
                { @"c:\solution\Test.NetStandard.csproj3", new MockFileData(MockFileStrings.GetTestNetStandardCsprojUnsorted(), Encoding.UTF8) }
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem);
            di.ServiceCollection.AddSingleton<CommandBaseTest>();
            var provider = di.CreateServiceProvider();
            var command = provider.GetRequiredService<CommandBaseTest>();
            command.Path = path;

            // act
            var result = await command.OnExecute();

            // assert
            Assert.AreEqual(0, command.AllFiles.Count);
            Assert.AreEqual(0, command.FileProjects.Count);
            Assert.AreEqual(0, command.FileProps.Count);
            Assert.AreEqual(path, command.Path);
            Assert.AreEqual(ErrorCodes.FileDoNotExists, result);
        }

        [TestMethod]
        public async Task TestCommandBaseDirectoryDoNotExistsAsync()
        {
            // arrange
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem);
            di.ServiceCollection.AddSingleton<CommandBaseTest>();
            var provider = di.CreateServiceProvider();
            var command = provider.GetRequiredService<CommandBaseTest>();
            command.Path = path;

            // act
            var result = await command.OnExecute();

            // assert
            Assert.AreEqual(path, command.Path);
            Assert.AreEqual(ErrorCodes.DirectoryDoNotExists, result);
        }

        [TestMethod]
        public async Task TestCommandBaseSetPathToExecutionAsync()
        {
            // arrange
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem);
            di.ServiceCollection.AddSingleton<CommandBaseTest>();
            var provider = di.CreateServiceProvider();
            var command = provider.GetRequiredService<CommandBaseTest>();

            // act
            var result = await command.OnExecute();

            // assert
            Assert.AreEqual(pathOfExecution, command.Path);
            Assert.AreEqual(ErrorCodes.FileDoNotExists, result);
        }

    }
}
