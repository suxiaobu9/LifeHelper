namespace LifeHelper.Server.Service;

public class DeleteAccountService
{
    private readonly DeleteAccountRepository deleteAccountRepository;
    private readonly UnitOfWork unitOfWork;
    public DeleteAccountService(DeleteAccountRepository deleteAccountRepository,
        UnitOfWork unitOfWork)
    {
        this.deleteAccountRepository = deleteAccountRepository;
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 取得刪除帳務的資料
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    public async Task<DeleteAccount?> GetDeleteAccount(int accountId)
    {
        return await deleteAccountRepository.GetDeleteAccount(accountId);
    }

    /// <summary>
    /// 新增刪除帳務的確認資料
    /// </summary>
    /// <returns></returns>
    public async Task AddDeleteAccount(int accountId)
    {
        await deleteAccountRepository.AddAsync(new DeleteAccount
        {
            AccountId = accountId,
            Deadline = DateTime.UtcNow.AddMinutes(5),
        });
        await unitOfWork.CompleteAsync();
    }

}
