using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSortAndSyncRefs.Common
{
    internal abstract class DryRun
    {
        protected Program Program => Program.Instance;

        protected bool IsNoDryRun => !Program.IsDryRun;
        protected bool IsDryRun => Program.IsDryRun;
    }
}
