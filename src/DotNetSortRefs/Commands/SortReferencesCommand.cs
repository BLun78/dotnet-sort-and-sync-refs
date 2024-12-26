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
    [Command("sort", "s", Description = "Sorts package references in all project files.")]
    internal class SortReferencesCommand : SortReferences, ICommandBase
    {
        public SortReferencesCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
