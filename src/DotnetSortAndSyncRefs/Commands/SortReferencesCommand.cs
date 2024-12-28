using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;
using DotnetSortAndSyncRefs.Common;

namespace DotnetSortAndSyncRefs.Commands
{
    [Command("sort", "s", 
        Description = "Sorts package references in all project files.")]
    internal class SortReferencesCommand : SortReferences, ICommandBase
    {
        [Argument(0, Description =
            "The path to a .csproj, .vbproj, .fsproj or directory. If a directory is specified, all .csproj, .vbproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
        public override string Path { get; set; }

        public SortReferencesCommand(IServiceProvider serviceProvider) 
            : base(serviceProvider, "sort")
        {
        }
    }
}
