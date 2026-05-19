namespace EliteAcademy.Application.Common.Interfaces
{
    public interface IAsyncQueryExecutor
    {
        Task<List<T>> ToListAsync<T>(IQueryable<T> query, bool noTracking = false, CancellationToken ct = default) where T : class;
        Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, bool noTracking = false, CancellationToken ct = default) where T : class;
        Task<T> SingleAsync<T>(IQueryable<T> query, bool noTracking = false, CancellationToken ct = default) where T : class;
        Task<T?> SingleOrDefaultAsync<T>(IQueryable<T> query, bool noTracking = false, CancellationToken ct = default) where T : class;
        Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken ct = default);
        Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken ct = default);
    }
}
