using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LifeHelper.Server.Repositories;

public class Repository<TEntity> where TEntity : class
{
    private readonly LifeHelperContext db;

    public Repository(LifeHelperContext db)
    {
        this.db = db;
    }

    public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate, bool isAsNoTracking = false)
    {
        var query = db.Set<TEntity>().Where(predicate);
        if (isAsNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return db.Set<TEntity>().AnyAsync(predicate);
    }

    public async Task AddAsync(TEntity entity)
    {
        await db.Set<TEntity>().AddAsync(entity);
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        return db.Set<TEntity>().AddRangeAsync(entities);
    }

    public void Remove(TEntity entity)
    {
        db.Set<TEntity>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        db.Set<TEntity>().RemoveRange(entities);
    }

    public IEnumerable<TEntity> GetByTSQL(string sqlQuery)
    {
        return db.Set<TEntity>().FromSqlRaw(sqlQuery);
    }

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return db.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

}
