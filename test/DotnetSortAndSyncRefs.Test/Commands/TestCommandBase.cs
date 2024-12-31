using DotnetSortAndSyncRefs.Test.Mocks;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSortAndSyncRefs.Commands;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Test.TestContend.CommandBase.TestCommandBaseCtorOk;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DotnetSortAndSyncRefs.Test.Commands
{
    [TestClass]
    public class TestCommandBase
    {
        private class CommandBaseTest : CommandBase
        {
            public CommandBaseTest(IServiceProvider serviceProvider)
                : base(serviceProvider, "CommandBaseTest")
            {
            }
        }

        [TestMethod]
        public async Task TestCommandBaseCtorOkAsync()
        {
            // arrange
            var reporter = Substitute.For<IReporter>();
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\solution\Directory.Packages.props", new MockFileData(MockFileStrings.GetDirectoryPackagesProps()) },
                { @"c:\solution\Test.Dotnet.csproj", new MockFileData(MockFileStrings.GetTestDotnetCsproj()) },
                { @"c:\solution\Test.NetStandard.csproj", new MockFileData(MockFileStrings.GetTestNetStandardCsproj()) }
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem, reporter);
            di.ServiceCollection.AddSingleton<CommandBaseTest>();
            var provider = di.CreateServiceProvider();
            var command = provider.GetRequiredService<CommandBaseTest>();
            command.Path = path;

            // act
            var result = await command.OnExecute();

            // assert
            Assert.AreEqual(3, command.AllFiles.Count);
            Assert.AreEqual(2, command.FileProjects.Count);
            Assert.AreEqual(1, command.FileProps.Count);
            Assert.AreEqual(path, command.Path);
            Assert.AreEqual(ErrorCodes.Ok, result);
        }

        [TestMethod]
        public async Task TestCommandBaseCtorFileDoNotExistsAsync()
        {
            // arrange
            var reporter = Substitute.For<IReporter>();
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\solution\Directory.Packages.props2", new MockFileData(MockFileStrings.GetDirectoryPackagesProps()) },
                { @"c:\solution\Test.Dotnet.csproj2", new MockFileData(MockFileStrings.GetTestDotnetCsproj()) },
                { @"c:\solution\Test.NetStandard.csproj2", new MockFileData(MockFileStrings.GetTestNetStandardCsproj()) }
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem, reporter);
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
        public async Task TestCommandBaseCtorDirectoryDoNotExistsAsync()
        {
            // arrange
            var reporter = Substitute.For<IReporter>();
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem, reporter);
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
        public async Task TestCommandBaseCtorSetPathToExecutionAsync()
        {
            // arrange
            var reporter = Substitute.For<IReporter>();
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem, reporter);
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
