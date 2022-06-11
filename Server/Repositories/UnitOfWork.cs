using Microsoft.EntityFrameworkCore.Storage;

namespace LifeHelper.Server.Repositories;

public class UnitOfWork
{
    private readonly LifeHelperContext db;

    public UnitOfWork(LifeHelperContext db)
    {
        this.db = db;
    }

    public Task CompleteAsync()
    {
        return db.SaveChangesAsync();
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return db.Database.BeginTransactionAsync();
    }
}
