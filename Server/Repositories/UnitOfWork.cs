using Microsoft.EntityFrameworkCore.Storage;

namespace LifeHelper.Server.Repositories;

public class UnitOfWork
{
    private readonly LifeHelperContext db;

    public UnitOfWork(LifeHelperContext db)
    {
        this.db = db;
    }

    public async Task CompleteAsync()
    {
        await db.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await db.Database.BeginTransactionAsync();
    }
}
