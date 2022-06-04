namespace LifeHelper.Server.Service;

public class DeleteAccountService
{
    private readonly DeleteConfirmRepository deleteConfirmRepository;
    private readonly UnitOfWork unitOfWork;
    public DeleteAccountService(DeleteConfirmRepository deleteAccountRepository,
        UnitOfWork unitOfWork)
    {
        this.deleteConfirmRepository = deleteAccountRepository;
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 取得刪除帳務的資料
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    public async Task<DeleteConfirm?> GetDeleteConfirm(int accountId)
    {
        return await deleteConfirmRepository.GetDeleteConfirm(accountId);
    }
}
