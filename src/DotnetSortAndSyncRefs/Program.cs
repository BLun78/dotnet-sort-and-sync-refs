using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotnetSortAndSyncRefs.Commands;
using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.NugetSpace;
using DotnetSortAndSyncRefs.Services;
using DotnetSortAndSyncRefs.Xml;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace DotnetSortAndSyncRefs
{
    [Command(
        Name = "dotnet sort-and-sync-refs",
        FullName = "A .NET Core global tool to alphabetically sort package references, create central package management in csproj, vbproj or fsproj.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [Subcommand(typeof(CentralPackageManagementCommand))]
    [Subcommand(typeof(InspectorCommand))]
    [Subcommand(typeof(SortReferencesCommand))]
    [Subcommand(typeof(SyncPackagesCommand))]
    [Subcommand(typeof(NuGetUpdateCommand))]
    [Subcommand(typeof(DotnetUpgradeCommand))]
    internal class Program
    {

        static async Task<int> Main(string[] args)
        {
            var provider = new ServiceCollection()
                // Reporter, Console and Commands
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .AddSingleton<global::DotnetSortAndSyncRefs.Common.IReporter>(provider => new Reporter(provider.GetRequiredService<IConsole>()!))
                .AddSingleton<IFileSystem, FileSystem>()

                // Nuget 
                .AddTransient<NuGetService>()
                .AddSingleton<SourceCacheContext>()
                .AddSingleton<NuGetRepository>()
                .AddSingleton<SourceRepository>(provider =>
                {
                    var sourceUri = "https://api.nuget.org/v3/index.json";

                    var packageSource = new PackageSource(sourceUri)
                    {
                        Credentials = new PackageSourceCredential(
                            source: sourceUri,
                            username: "myUsername",
                            passwordText: "myVerySecretPassword",
                            isPasswordClearText: true,
                            validAuthenticationTypesText: null
                            )
                    };
                    return Repository.Factory.GetCoreV3(packageSource);
                })
                .AddSingleton<ILogger, NuGetLogger>()

                // Commands
                .AddCommands()

                // XML Files
                .AddXmlFiles()

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

        private static string GetVersion() => $"{typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? string.Empty} - {GetBuildDate} UTC";

        private static string GetBuildDate => DateTime.ParseExact(typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyMetadataAttribute>()?
            .Value ?? string.Empty
            , "yyyyMMddHHmmss", new DateTimeFormatInfo(), DateTimeStyles.AdjustToUniversal).ToString(CultureInfo.InvariantCulture);

        protected async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // default command
            await app.ExecuteAsync(new[] { "sort" }, CancellationToken.None).ConfigureAwait(false);

            return ErrorCodes.Ok;
        }
    }
}
