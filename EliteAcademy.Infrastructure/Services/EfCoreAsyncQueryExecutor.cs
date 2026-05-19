using EliteAcademy.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EliteAcademy.Infrastructure.Services
{
    public class EfCoreAsyncQueryExecutor : IAsyncQueryExecutor
    {
        public Task<List<T>> ToListAsync<T>(IQueryable<T> query, bool noTracking = false, CancellationToken ct = default) where T : class =>
            (noTracking ? query.AsNoTracking() : query).ToListAsync(ct);

        public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, bool noTracking = false, CancellationToken ct = default) where T : class =>
            (noTracking ? query.AsNoTracking() : query).FirstOrDefaultAsync(ct);

        public Task<T> SingleAsync<T>(IQueryable<T> query, bool noTracking = false, CancellationToken ct = default) where T : class =>
            (noTracking ? query.AsNoTracking() : query).SingleAsync(ct);

        public Task<T?> SingleOrDefaultAsync<T>(IQueryable<T> query, bool noTracking = false, CancellationToken ct = default) where T : class =>
            (noTracking ? query.AsNoTracking() : query).SingleOrDefaultAsync(ct);

        public Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken ct = default) =>
            query.AnyAsync(ct);

        public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken ct = default) =>
            query.CountAsync(ct);
    }
}
