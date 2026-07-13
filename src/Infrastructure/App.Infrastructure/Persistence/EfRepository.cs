using System.Linq.Expressions;
using App.Application.Abstractions;
using App.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

/// <summary>
/// EF Core tabanlı generic repository implementasyonu.
/// </summary>
public class EfRepository<T>(AppDbContext dbContext) : IRepository<T>
    where T : BaseEntity, IAggregateRoot
{
    protected readonly DbSet<T> DbSet = dbContext.Set<T>();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default) =>
        DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public Task<List<T>> ListAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        DbSet.AnyAsync(predicate, cancellationToken);

    public IQueryable<T> Query() => DbSet.AsQueryable();

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);
}
