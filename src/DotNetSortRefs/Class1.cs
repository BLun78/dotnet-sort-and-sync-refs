using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSortAndSyncRefs
{
    internal class Processor
    {
        private readonly IServiceProvider _serviceProvider;

        public Processor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


    }
}
