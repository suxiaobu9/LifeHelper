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
    public async Task<User[]> GetUsers(string[] userLineIds)
    {
        return await Where(x => userLineIds.Contains(x.LineUserId)).ToArrayAsync();
    }

    public async Task<User?> GetUser(string userLineId)
    {
        return await FirstOrDefaultAsync(x => x.LineUserId == userLineId);
    }

}
