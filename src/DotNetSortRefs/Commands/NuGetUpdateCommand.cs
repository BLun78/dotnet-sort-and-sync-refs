using DotnetSortAndSyncRefs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace DotnetSortAndSyncRefs.Commands
{
    [Command("nuget-update", "update", "ud", "u", Description = "Updates NuGet packages in all project files.")]
    internal class NuGetUpdateCommand : CommandBase, ICommandBase
    {
        public NuGetUpdateCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override Task<int> OnExecute()
        {
            var result = ErrorCodes.NuGetUpdateIsFailed;
            throw new NotImplementedException();
        }
    }
}
