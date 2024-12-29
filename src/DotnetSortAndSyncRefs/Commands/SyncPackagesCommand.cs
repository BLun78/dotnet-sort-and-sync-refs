using DotnetSortAndSyncRefs.Common;
using DotnetSortAndSyncRefs.Xml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotnetSortAndSyncRefs.Extensions;
using McMaster.Extensions.CommandLineUtils;

namespace DotnetSortAndSyncRefs.Commands
{
    [Command("sync", "sy",
        Description = "Syncs package references in all project files.")]
    internal class SyncPackagesCommand : SyncPackages, ICommandBase
    {
        [Argument(0, Description =
            "The path to a .csproj, .vbproj, .fsproj or directory. If a directory is specified, all .csproj, .vbproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
        public override string Path { get; set; }

        public SyncPackagesCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "sync")
        {
        }
    }
}
