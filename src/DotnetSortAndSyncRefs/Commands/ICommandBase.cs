using System.Threading.Tasks;

namespace DotnetSortAndSyncRefs.Commands;

internal interface ICommandBase
{
    Task<int> OnExecute();
}