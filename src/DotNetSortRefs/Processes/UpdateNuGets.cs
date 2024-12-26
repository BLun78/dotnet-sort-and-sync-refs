using DotnetSortAndSyncRefs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSortAndSyncRefs.Processes
{
    internal class NuGetUpdate
    {
        private readonly IServiceProvider _serviceProvider;

        public NuGetUpdate(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<int> UpdateNuGets(List<string> fileProjects, List<string> fileProps)
        {

            var result = ErrorCodes.NuGetUpdateIsFailed;


        }
    }
}
