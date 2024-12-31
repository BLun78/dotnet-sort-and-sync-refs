using DotnetSortAndSyncRefs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace DotnetSortAndSyncRefs.Commands
{
    [Command("nuget-update", "update", "ud", "u",
        Description = "Updates NuGet packages in all project files.")]
    internal class NuGetUpdateCommand : CommandBase, ICommandBase
    {
        [Argument(0, Description =
            "The path to a .csproj, .vbproj, .fsproj or directory. If a directory is specified, all .csproj, .vbproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
        public override string Path { get; set; }

        public NuGetUpdateCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "nuget-update")
        {
        }

        public override async Task<int> OnExecute()
        {
            var result = await base.OnExecute();
            if (result != ErrorCodes.Ok)
            {
                return result;
            }

            throw new NotImplementedException();
        }
    }
}
