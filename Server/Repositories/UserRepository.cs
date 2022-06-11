using Microsoft.EntityFrameworkCore;

namespace LifeHelper.Server.Repositories;

public class UserRepository : Repository<User>
{
    public UserRepository(LifeHelperContext db) : base(db) { }

    /// <summary>
    /// 取得使用者
    /// </summary>
    /// <param name="userLineIds"></param>
    /// <returns></returns>
    public Task<User[]> GetUsersAsync(string[] userLineIds)
    {
        return Where(x => userLineIds.Contains(x.LineUserId)).ToArrayAsync();
    }

    public Task<User?> GetUserAsync(string userLineId)
    {
        return FirstOrDefaultAsync(x => x.LineUserId == userLineId);
    }

}
