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
using DotnetSortAndSyncRefs.NugetSpace;
using DotnetSortAndSyncRefs.Processes;
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
        public static Program Instance { get; private set; }

        static async Task<int> Main(string[] args)
        {
            var provider = new ServiceCollection()
                // Reporter, Console and Commands
                .AddSingleton(PhysicalConsole.Singleton)
                .AddSingleton<Reporter>(provider => new Reporter(provider.GetRequiredService<IConsole>()!))
                .AddSingleton<IFileSystem, FileSystem>()

                // Processors
                .AddSingleton<Processor>()
                .AddSingleton<CentralPackageManagement>()
                .AddSingleton<SyncPackageVersions>()
                .AddSingleton<SortReferences>()
                .AddSingleton<Inspector>()

                // Nuget 
                .AddSingleton<SourceCacheContext>()
                .AddSingleton<NuGetRepository>()
                .AddSingleton<SourceRepository>(provider => Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json"))
                .AddSingleton<ILogger, NuGetLogger>()

                // XML Files
                .AddTransient<XmlAllElementFile>()
                .AddTransient<XmlProjectFile>()
                .AddTransient<XmlCentralPackageManagementFile>()

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
                return ErrorCodes.ApplicationCriticalError;
            }
        }

        [Argument(0, Description =
            "The path to a .csproj, .vbproj, .fsproj or directory. If a directory is specified, all .csproj, .vbproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
        public string Path { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to inspect and return a non-zero exit code if one or more projects have non-sorted package references.",
            ShortName = "i", LongName = "inspect")]
        public bool IsInspect { get; set; } = false;

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to remove not needed PackageVersion to protect the application for old version usage.",
            ShortName = "cl", LongName = "clean")]
        public bool DoCleanUpPackageVersions { get; set; } = false;

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to enable Central Package Management and create a file called \"Directory.Packages.props\".",
            ShortName = "c", LongName = "create")]
        public bool DoCreatePackageVersions { get; set; } = false;

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to do a dry run. It shows the effected actions, but do not change the files.",
            ShortName = "dr", LongName = "dry-run")]
        public bool IsDryRun { get; set; } = false;

        private static string GetVersion() => $"{typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion} - {GetBuildDate} UTC";

        private static string GetBuildDate => DateTime.ParseExact(typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyMetadataAttribute>()
            ?.Value
            , "yyyyMMddHHmmss", new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal).ToString();

        private readonly Processor _processor;
        private readonly IFileSystem _fileSystem;
        private readonly Reporter _reporter;
        private Commands _command;

        public Program(
            Processor processor,
            IFileSystem fileSystem,
            Reporter reporter)
        {
            _processor = processor;
            _fileSystem = fileSystem;
            _reporter = reporter;
            Program.Instance = this;
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
                    return ErrorCodes.DirectoryDoNotExists;
                }

                if (IsInspect)
                {
                    _command = Commands.Inspect;
                }
                else if (DoCreatePackageVersions)
                {
                    _command = Commands.Create;
                }
                else if (DoCleanUpPackageVersions)
                {
                    _command = Commands.CLean;
                }
                else
                {
                    // Default is Sort References
                    _command = Commands.Sort;
                }

                return await _processor.Process(_command);
            }
            catch (Exception e)
            {
                _reporter.Error(e.StackTrace!);
                return ErrorCodes.CriticalError;
            }
        }
    }
}
