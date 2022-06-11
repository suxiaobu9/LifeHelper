﻿namespace LifeHelper.Server.Repositories
{
    public class DeleteConfirmRepository : Repository<DeleteConfirm>
    {
        public DeleteConfirmRepository(LifeHelperContext db) : base(db) { }

        /// <summary>
        /// 取得刪除帳務的資料
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Task<DeleteConfirm?> GetDeleteConfirmAsync(int id)
        {
            return FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
