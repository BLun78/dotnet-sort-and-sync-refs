using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetSortRefs
{
    [Command(
        Name = "dotnet sort-refs",
        FullName = "A .NET Core global tool to alphabetically sort package references in csproj or fsproj.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    internal class Program : CommandBase
    {
        static async Task<int> Main(string[] args)
        {
            var provider = new ServiceCollection()
                .AddSingleton(PhysicalConsole.Singleton)
                .AddSingleton<SyncRefs>()
                .AddSingleton<IReporter>(provider => new ConsoleReporter(provider.GetService<IConsole>()!))
                .AddSingleton<IFileSystem, FileSystem>()
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
            "The path to a .csproj, .fsproj or directory. If a directory is specified, all .csproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
        public string Path { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to inspect and return a non-zero exit code if one or more projects have non-sorted package references.",
            ShortName = "i", LongName = "inspect")]
        public bool IsInspect { get; set; } = false;

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to cleanup the PackageVersion to protect the application for old version usage.",
            ShortName = "c", LongName = "cleanup")]
        public bool IsCleanup { get; set; } = false;


        private static string GetVersion() => typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        private readonly IFileSystem _fileSystem;
        private readonly IReporter _reporter;

        public Program(IFileSystem fileSystem, IReporter reporter)
        {
            _fileSystem = fileSystem;
            _reporter = reporter;
        }

        private async Task<int> OnExecute(CommandLineApplication app, IConsole console)
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

                var extensionsProjects = new[] { ".csproj", ".fsproj", ".vbproj" };
                var extensionsProps = new[] { ".props" };
                var allExtensions = new List<string>();
                allExtensions.AddRange(extensionsProjects);
                allExtensions.AddRange(extensionsProps);

                var allFiles = LoadFilesFromExtension(allExtensions);

                if (allFiles.Count == 0)
                {
                    _reporter.Error($"no '{string.Join(", ", allExtensions)}'' files found.");
                    return 1;
                }

                var projFilesWithNonSortedReferences = await XmlHelper
                    .Inspect(allFiles)
                    .ConfigureAwait(false);

                if (IsInspect)
                {
                    Console.WriteLine("Running inspection...");
                    PrintInspectionResults(allFiles, projFilesWithNonSortedReferences);
                    return projFilesWithNonSortedReferences.Count > 0 ? 1 : 0;
                }
                else if (IsCleanup)
                {
                    Console.WriteLine("Running cleanup...");
                    var fileProjects = LoadFilesFromExtension(extensionsProjects);
                    var fileProps = LoadFilesFromExtension(extensionsProps);
                    return await SyncRefs.CleanUp(fileProjects, fileProps).ConfigureAwait(false);
                }
                else
                {
                    Console.WriteLine("Running sort package references...");
                    return await XmlHelper
                        .SortReferences(projFilesWithNonSortedReferences, _reporter)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _reporter.Error(e.StackTrace!);
                return 1;
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
            IEnumerable<string> projFiles,
            ICollection<string> projFilesWithNonSortedReferences)
        {
            foreach (var proj in projFiles)
            {
                if (projFilesWithNonSortedReferences.Contains(proj))
                {
                    _reporter.Error($"» {proj} X");
                }
                else
                {
                    _reporter.Output($"» {proj} ✓");
                }
            }
        }
    }
}
