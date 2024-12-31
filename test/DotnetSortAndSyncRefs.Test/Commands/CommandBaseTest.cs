using DotnetSortAndSyncRefs.Commands;

namespace DotnetSortAndSyncRefs.Test.Commands;

internal class CommandBaseTest : CommandBase
{
    public CommandBaseTest(IServiceProvider serviceProvider)
        : base(serviceProvider, "CommandBaseTest")
    {
    }
}