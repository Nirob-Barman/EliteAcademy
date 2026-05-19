namespace EliteAcademy.Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        //IRepository<T> Repository<T>() where T : class;
        //Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        //Task BeginTransaction();
        //Task CommitAsync();
        //Task RollbackAsync();
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
