using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetSortAndSyncRefs.Commands;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Test.Mocks;
using DotnetSortAndSyncRefs.Test.TestContend.CommandBase.TestCommandBaseCtorOk;
using DotnetSortAndSyncRefs.Xml;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using IReporter = DotnetSortAndSyncRefs.Common.IReporter;

namespace DotnetSortAndSyncRefs.Test.Commands
{
    [TestClass]
    public sealed class TestSortReferencesCommand
    {
        [TestMethod]
        public async Task Test_SortReferencesCommand_Ok()
        {
            // arrange
            var path = @"c:\solution";
            var pathOfExecution = @"c:\execution\";
            var pathOfResultFile = @"c:\solution\Test.Dotnet.csproj";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\solution\Directory.Packages.props", new MockFileData(MockFileStrings.GetDirectoryPackagesPropsUnsorted(), Encoding.UTF8) },
                { pathOfResultFile, new MockFileData(MockFileStrings.GetTestDotnetCsprojUnsorted(), Encoding.UTF8) },
                { @"c:\solution\Test.NetStandard.csproj", new MockFileData(MockFileStrings.GetTestNetStandardCsprojUnsorted(), Encoding.UTF8) }
            }, pathOfExecution);

            var di = new DependencyInjectionMock(fileSystem);
            var provider = di.CreateServiceProvider();
            var command = provider.GetRequiredService<SortReferencesCommand>();
            var reporter = provider.GetRequiredService<IReporter>();
            var xmlResultFileBefor = provider.GetRequiredService<XmlAllElementFile>();
            await xmlResultFileBefor.LoadFileAsync(pathOfResultFile, false, false, false);
            var xmlResultFileResult = provider.GetRequiredService<XmlAllElementFile>();
            command.Path = path;
            reporter.Output("Input File:");
            reporter.Output(xmlResultFileBefor.ToString());

            // act
            var result = await command.OnExecute();

            // assert
            Assert.AreEqual(3, command.ProjFilesWithNonSortedReferences.Count); // result of Inspection
            Assert.AreEqual(3, command.AllFiles.Count);
            Assert.AreEqual(2, command.FileProjects.Count);
            Assert.AreEqual(1, command.FileProps.Count);
            Assert.AreEqual(path, command.Path);
            Assert.AreEqual(ErrorCodes.Ok, result);

            Assert.IsTrue(fileSystem.FileExists(pathOfResultFile));

            await xmlResultFileResult.LoadFileAsync(pathOfResultFile, false, false,false);
            Assert.AreEqual(3, xmlResultFileResult.ItemGroups.ToList().Count);
            reporter.Output("Result File:");
            reporter.Output(xmlResultFileResult.ToString());
        }
    }

}
