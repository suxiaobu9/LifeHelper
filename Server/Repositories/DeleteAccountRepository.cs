namespace LifeHelper.Server.Repositories
{
    public class DeleteAccountRepository : Repository<DeleteAccount>
    {
        public DeleteAccountRepository(LifeHelperContext db) : base(db) { }

        /// <summary>
        /// 取得刪除帳務的資料
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<DeleteAccount?> GetDeleteAccount(int accountId)
        {
            return await FirstOrDefaultAsync(x => x.AccountId == accountId);
        }
    }
}
