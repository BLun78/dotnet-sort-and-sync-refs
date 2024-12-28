using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Xml;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace DotnetSortAndSyncRefs.Commands
{
    [HelpOption]
    internal abstract class CommandBase : ICommandBase
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly IFileSystem FileSystem;
        protected readonly Reporter Reporter;
        protected List<string> AllExtensions { get; private set; }
        protected List<string> FileProps { get; private set; }
        protected List<string> FileProjects { get; private set; }
        protected List<string> AllFiles { get; private set; }
        protected List<string> ProjFilesWithNonSortedReferences { get; private set; }

        protected HashSet<string> ProjectFilePostfix = new(StringComparer.OrdinalIgnoreCase) {
            ".csproj",
            ".vbproj",
            ".fsproj",
        };
        protected HashSet<string> AdditionalFilePostfix = new(StringComparer.OrdinalIgnoreCase) {
            ".props"
        };

        protected readonly string CentralPackageManagementFile = "Directory.Packages.props";

        public virtual string Path { get; set; }

        [Option(CommandOptionType.NoValue,
            Description =
                "Specifies whether to do a dry run. It shows the effected actions, but do not change the files.",
            ShortName = "dr", LongName = "dry-run")]
        public bool IsDryRun { get; set; } = false;

        public bool IsNoDryRun => !IsDryRun;

        protected CommandBase(IServiceProvider serviceProvider, string commandStartMessage)
        {
            ServiceProvider = serviceProvider;
            FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
            Reporter = serviceProvider.GetRequiredService<Reporter>();

            Reporter.Output($"start command: {commandStartMessage}");

            if (string.IsNullOrEmpty(Path))
            {
                Path = FileSystem
                    .Directory
                    .GetCurrentDirectory();
            }

            if (!(FileSystem.File.Exists(Path) ||
                  FileSystem.Directory.Exists(Path)))
            {
                Reporter.Error("Directory or file does not exist.");
                Environment.Exit(ErrorCodes.DirectoryDoNotExists);
            }

            AllExtensions = new List<string> { };
            AllExtensions.AddRange(ProjectFilePostfix);
            AllExtensions.AddRange(AdditionalFilePostfix);

            FileProjects = LoadFilesFromExtension(ProjectFilePostfix);
            FileProps = LoadFilesFromExtension(AdditionalFilePostfix);

            AllFiles = new List<string> { };
            AllFiles.AddRange(FileProjects);
            AllFiles.AddRange(FileProps);

            if (AllFiles.Count == 0)
            {
                Reporter.Error($"no '{string.Join(", ", AllExtensions)}'' files found.");
                Environment.Exit(ErrorCodes.FileDoNotExists);
            }

            Reporter.Output("Running analysis ...");
            ProjFilesWithNonSortedReferences = Inspect();

            if (ProjFilesWithNonSortedReferences == null)
            {
                Reporter.Do("Please solve the issue of the Project file(s).");
                Environment.Exit(ErrorCodes.ProjectFileHasNotAValidXmlFormat);
            }
        }

        public abstract Task<int> OnExecute();

        public async Task<List<string>> InspectAsync()
        {
            var projFilesWithNonSortedReferences = new List<string>();

            foreach (var projFile in AllFiles)
            {
                try
                {
                    var xmlFile = ServiceProvider.GetRequiredService<XmlAllElementFile>();
                    await xmlFile
                        .LoadFileReadOnlyAsync(projFile)
                        .ConfigureAwait(false);

                    foreach (var itemGroup in xmlFile.ItemGroups)
                    {
                        var references = itemGroup
                            .XPathSelectElements(ConstConfig.AllElementTypesQuery)
                            .Select(x => x.Attribute("Include")?.Value.ToLowerInvariant())
                            .ToList();

                        if (references.Count <= 1) continue;

                        var sortedReferences = references
                            .OrderBy(x => x)
                            .ToList();

                        var result = references.SequenceEqual(sortedReferences);

                        if (!result && !projFilesWithNonSortedReferences.Contains(projFile))
                        {
                            Reporter.NotOk($"» {projFile}");
                            projFilesWithNonSortedReferences.Add(projFile);
                        }
                        else if (!projFilesWithNonSortedReferences.Contains(projFile))
                        {
                            Reporter.Ok($"» {projFile}");
                            projFilesWithNonSortedReferences.Add(projFile);
                        }
                    }
                }
                catch (Exception e)
                {
                    Reporter.Error($"» {projFile}");
                    Reporter.Error(e.Message);
                    return null;
                }
            }

            return projFilesWithNonSortedReferences;
        }

        private List<string> Inspect()
        {
            return InspectAsync()
                .ConfigureAwait(true).GetAwaiter().GetResult();
        }

        private List<string> LoadFilesFromExtension(IEnumerable<string> extensions)
        {
            var projFiles = new List<string>();
            if (FileSystem.File.Exists(Path))
            {
                projFiles.Add(Path);
            }
            else
            {
                projFiles = extensions
                    .SelectMany(ext => FileSystem
                        .Directory
                        .GetFiles(Path, $"*{ext}", SearchOption.AllDirectories))
                    .ToList();
            }

            return projFiles;
        }
    }
}
