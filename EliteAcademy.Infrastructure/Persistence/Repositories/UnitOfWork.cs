using EliteAcademy.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
namespace EliteAcademy.Infrastructure.Persistence.Repositories
{
    //public class UnitOfWork : IUnitOfWork
    public class UnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        //public IRepository<T> Repository<T>() where T : class
        //{
        //    var type = typeof(T);

        //    if (!_repositories.ContainsKey(type))
        //    {
        //        var repositoryInstance = new GenericRepository<T>(_context);
        //        _repositories.Add(type, repositoryInstance);
        //    }

        //    return (IRepository<T>)_repositories[type];
        //}

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            //if (_repositories.ContainsKey(type))
            //    return (IRepository<T>)_repositories[type];
            if (_repositories.TryGetValue(type, out var repo))
                return (IRepository<T>)repo;

            var repoInstance = new GenericRepository<T>(_context);
            _repositories[type] = repoInstance;
            return repoInstance;
        }


        // Begin a transaction
        public async Task BeginTransaction()
        {
            if (_transaction != null)
                throw new InvalidOperationException("A database transaction is already in progress. Cannot start a new one.");

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        // Commit the transaction
        public async Task CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Cannot commit because no transaction has been started.");

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch (Exception)
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null!;
            }
        }

        // Rollback the transaction
        public async Task RollbackAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Cannot rollback because no transaction has been started.");

            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null!;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
            }
            // Dispose of the DbContext
            _context.Dispose();
        }
    }
}
