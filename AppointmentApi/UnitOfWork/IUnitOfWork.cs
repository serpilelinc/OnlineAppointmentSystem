using System.Threading;
using System.Threading.Tasks;

namespace AppointmentApi.UnitOfWork
{
    public interface IUnitOfWork : System.IAsyncDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
