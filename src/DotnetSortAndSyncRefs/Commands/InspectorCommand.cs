using DotnetSortAndSyncRefs.Common;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSortAndSyncRefs.Commands
{
    [Command("inspect", "i", 
        Description = "Specifies whether to inspect and return a non-zero exit code if one or more projects have non-sorted package references.")]
    internal class InspectorCommand : CommandBase, ICommandBase
    {
        public InspectorCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "inspect")
        {
        }

        public override Task<int> OnExecute()
        {
            Reporter.Output("Running inspection ...");

            PrintInspectionResults(AllFiles, ProjFilesWithNonSortedReferences);

            var result = ProjFilesWithNonSortedReferences.Any()
                ? ErrorCodes.Ok
                : ErrorCodes.InspectionFoundNothing;

            return Task.FromResult(result);
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
                    Reporter.Error($"» {paddedProjectFile} - need to sort");
                }
                else
                {
                    Reporter.Ok($"» {paddedProjectFile} - Ok");
                }
            }
        }
    }
}
