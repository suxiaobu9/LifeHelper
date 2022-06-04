using Microsoft.EntityFrameworkCore;

namespace LifeHelper.Server.Repositories;

public class MemorandumRepository : Repository<Memorandum>
{
    public MemorandumRepository(LifeHelperContext db) : base(db) { }

    /// <summary>
    /// 取得備忘錄
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<Memorandum?> GetMemorandum(int id)
    {
        return FirstOrDefaultAsync(x => x.Id == id);
    }

/// <summary>
/// 取得使用者的備忘錄
/// </summary>
/// <param name="userId"></param>
/// <returns></returns>
    public Task<Memorandum[]> GetUserMemorandum(int userId)
    {
        return Where(x => x.UserId == userId).ToArrayAsync();
    }
}