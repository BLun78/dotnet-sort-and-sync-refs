using DotnetSortAndSyncRefs.Common;
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

        public async Task<int> Process(Commands command)
        {
            switch (command)
            {
                case Commands.Inspect:
                    break;
                case Commands.Remove:
                    break;
                case Commands.Create:
                    break;
                case Commands.Sort:
                    break;
                case Commands.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }

            throw new NotImplementedException();
        }
    }
}
