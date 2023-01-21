using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Server.Service.Interface;
using LifeHelper.Shared.Enum;

namespace LifeHelper.Server.Service.MsSql;

public class MemorandumService : IMemorandumService
{
    private readonly MemorandumRepository memorandumRepository;
    private readonly UnitOfWork unitOfWork;
    private readonly DeleteConfirmRepository deleteConfirmRepository;

    public MemorandumService(MemorandumRepository memorandumRepository,
        UnitOfWork unitOfWork,
        DeleteConfirmRepository deleteConfirmRepository)
    {
        this.memorandumRepository = memorandumRepository;
        this.unitOfWork = unitOfWork;
        this.deleteConfirmRepository = deleteConfirmRepository;
    }

    /// <summary>
    /// 備忘錄
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> RecordMemoAsync(string msg, int userId)
    {
        await AddMemoAsync(msg, userId);

        var userMemoes = await memorandumRepository.GetUserMemorandum(userId);

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.MemorandumFlexMessageTemplateAsync(userMemoes));
    }

    /// <summary>
    /// 新增備註
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public async Task<Memorandum> AddMemoAsync(string msg, int userId)
    {
        var memorandum = new Memorandum
        {
            Memo = msg,
            UserId = userId
        };

        await memorandumRepository.AddAsync(memorandum);

        await unitOfWork.CompleteAsync();

        return memorandum;
    }

    /// <summary>
    /// 刪除備忘錄
    /// </summary>
    /// <returns></returns>
    public async Task RemoveMemoAsync(int memoId, int userId)
    {
        var memo = await memorandumRepository.GetMemorandum(memoId, userId);
        var deleteConfirm = await deleteConfirmRepository.GetDeleteConfirmByFeatureIdAsync(memoId, userId);

        if (memo != null)
            memorandumRepository.Remove(memo);

        if (deleteConfirm != null)
            deleteConfirmRepository.Remove(deleteConfirm);

        await unitOfWork.CompleteAsync();
    }

    /// <summary>
    /// 取得使用者備忘錄
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<Memorandum[]> GetUserMemorandumAsync(int userId)
    {
        return memorandumRepository.GetUserMemorandum(userId);
    }

}
