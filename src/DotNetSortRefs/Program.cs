using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Xml;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace DotnetSortAndSyncRefs
{
    [Command(
        Name = "dotnet sort-and-sync-refs",
        FullName = "A .NET Core global tool to alphabetically sort package references, create central package management in csproj, vbproj or fsproj.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [HelpOption]
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var provider = new ServiceCollection()
                .AddSingleton(PhysicalConsole.Singleton)
                .AddSingleton<Reporter>(provider => new Reporter(provider.GetRequiredService<IConsole>()!))
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<SourceCacheContext>()
                .AddSingleton<NuGetRepository>()
                .AddSingleton<SourceRepository>(provider => Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json"))
                .AddSingleton<ILogger,NuGetLogger>()
                .BuildServiceProvider();

            await using var providerDisposeTask = provider.ConfigureAwait(false);

            var app = new CommandLineApplication<Program>
            {
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.Throw
            };

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(provider);

            try
            {
                return await app.ExecuteAsync(args)
                    .ConfigureAwait(false);
            }
            catch (UnrecognizedCommandParsingException)
            {
                app.ShowHelp();
                return 1;
            }
        }

        [Argument(0, Description =
            "The path to a .csproj, .vbproj, .fsproj or directory. If a directory is specified, all .csproj, .vbproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
        public string Path { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to inspect and return a non-zero exit code if one or more projects have non-sorted package references.",
            ShortName = "i", LongName = "inspect")]
        public bool IsInspect { get; set; } = false;

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to remove not needed PackageVersion to protect the application for old version usage.",
            ShortName = "r", LongName = "remove")]
        public bool DoRemovePackageVersions { get; set; } = false;

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to enable Central Package Management and create a file called \"Directory.Packages.props\".",
            ShortName = "c", LongName = "create")]
        public bool DoCreatePackageVersions { get; set; } = false;

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to do a dry run. It shows the effected actions, but do not change the files.",
            ShortName = "dr", LongName = "dry-run")]
        public bool DryRun { get; set; } = false;

        private HashSet<string> ProjectFilePostfix = new(StringComparer.OrdinalIgnoreCase) {
            ".csproj",
            ".vbproj",
            ".fsproj",
        };

        private HashSet<string> AdditionalFilePostfix = new(StringComparer.OrdinalIgnoreCase) {
            ".props",
            ".targets"
        };

        private static string GetVersion() => $"{typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion} - {GetBuildDate} UTC";

        private static string GetBuildDate => DateTime.ParseExact(typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyMetadataAttribute>()
            ?.Value
            , "yyyyMMddHHmmss", new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal).ToString();

        private readonly IFileSystem _fileSystem;
        private readonly Reporter _reporter;
        private readonly IConsole _console;
        private readonly NuGetRepository _nuGetUpdateVersion;

        public Program(
            IFileSystem fileSystem, 
            Reporter reporter, 
            IConsole console,
            NuGetRepository nuGetUpdateVersion)
        {
            _fileSystem = fileSystem;
            _reporter = reporter;
            _console = console;
            _nuGetUpdateVersion = nuGetUpdateVersion;
        }

        private async Task<int> OnExecute(CommandLineApplication app)
        {
            try
            {
                if (string.IsNullOrEmpty(Path))
                    Path = _fileSystem
                        .Directory
                        .GetCurrentDirectory();

                if (!(_fileSystem.File.Exists(Path) ||
                      _fileSystem.Directory.Exists(Path)))
                {
                    _reporter.Error("Directory or file does not exist.");
                    return 1;
                }

                var result = -10;
                var res= await _nuGetUpdateVersion.GetAllVersionsAsync("Microsoft.Extensions.DependencyInjection");
                var res2= await _nuGetUpdateVersion.GetMetadataAsync("Microsoft.Extensions.DependencyInjection");

                var allExtensions = new List<string>();
                allExtensions.AddRange(ProjectFilePostfix);
                allExtensions.AddRange(AdditionalFilePostfix);

                var fileProjects = LoadFilesFromExtension(ProjectFilePostfix);
                var fileProps = LoadFilesFromExtension(AdditionalFilePostfix);
                var allFiles = new List<string>();
                allFiles.AddRange(fileProjects);
                allFiles.AddRange(fileProps);

                if (allFiles.Count == 0)
                {
                    _reporter.Error($"no '{string.Join(", ", allExtensions)}'' files found.");
                    return 2;
                }

                _reporter.Output("Running analysis ...");
                var projFilesWithNonSortedReferences = await _fileSystem
                    .Inspect(_reporter, allFiles)
                    .ConfigureAwait(false);

                if (projFilesWithNonSortedReferences == null)
                {
                    _reporter.Do("Please solve the issue of the Project file(s).");
                    return 4;
                }

                if (IsInspect)
                {
                    _reporter.Output("Running inspection ...");
                    PrintInspectionResults(allFiles, projFilesWithNonSortedReferences);
                    result = projFilesWithNonSortedReferences.Count > 0
                        ? 0
                        : 1;
                }
                else if (DoRemovePackageVersions)
                {
                    _reporter.Output("Running remove not needed PackageVersion ...");
                    result = await _fileSystem
                        .RemovePackageVersions(_reporter, fileProjects, fileProps, DryRun)
                        .ConfigureAwait(false);
                    if (result == 0)
                    {
                        result = await _fileSystem
                            .SortReferences(_reporter, projFilesWithNonSortedReferences, DryRun)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        result = -1;
                    }
                }
                else if (DoCreatePackageVersions)
                {
                    _reporter.Output("Running create a Central Package Management file ( \"Directory.Packages.props\") ...");
                    result = await _fileSystem
                        .CreatePackageVersions(_reporter, fileProjects, Path, DryRun)
                        .ConfigureAwait(false);

                    if (result == 0)
                    {
                        result = await _fileSystem
                            .SortReferences(_reporter, projFilesWithNonSortedReferences, DryRun)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        result = -2;
                    }
                }
                else
                {
                    result = await _fileSystem
                        .SortReferences(_reporter, projFilesWithNonSortedReferences, DryRun)
                        .ConfigureAwait(false);
                }

                _reporter.Output("Done.");
                return result;
            }
            catch (Exception e)
            {
                _reporter.Error(e.StackTrace!);
                return -3;
            }
        }

        private List<string> LoadFilesFromExtension(IEnumerable<string> extensions)
        {
            var projFiles = new List<string>();
            if (_fileSystem.File.Exists(Path))
            {
                projFiles.Add(Path);
            }
            else
            {
                projFiles = extensions
                    .SelectMany(ext => _fileSystem
                        .Directory
                        .GetFiles(Path, $"*{ext}", SearchOption.AllDirectories))
                    .ToList();
            }

            return projFiles;
        }

        private void PrintInspectionResults(
            ICollection<string> projFiles,
            ICollection<string> projFilesWithNonSortedReferences)
        {
            var max = projFiles
                .Max(x => x.Length);

            foreach (var proj in projFiles)
            {
                var paddedProjectFile = proj
                    .PadRight(max);

                if (projFilesWithNonSortedReferences.Contains(proj))
                {
                    _reporter.Error($"» {paddedProjectFile} - X");
                }
                else
                {
                    _reporter.Ok($"» {paddedProjectFile} - Ok");
                }
            }
        }
    }
}
