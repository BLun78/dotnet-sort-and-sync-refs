using System.Threading.Tasks;

namespace DotnetSortAndSyncRefs.Commands;

public interface ICommandBase
{
    Task<int> OnExecute();
}