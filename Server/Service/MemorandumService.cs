using LifeHelper.Server.Models.LineApi;
using LifeHelper.Server.Models.Template;
using LifeHelper.Shared.Enum;

namespace LifeHelper.Server.Service;

public class MemorandumService
{
    private readonly MemorandumRepository memorandumRepository;
    private readonly UnitOfWork unitOfWork;

    public MemorandumService(MemorandumRepository memorandumRepository,
        UnitOfWork unitOfWork)
    {
        this.memorandumRepository = memorandumRepository;
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 備忘錄
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public async Task<LineReplyModel> RecordMemo(string msg, int userId)
    {
        await AddMemo(msg, userId);

        var userMemoes = await memorandumRepository.GetUserMemorandum(userId);

        return new LineReplyModel(LineReplyEnum.Json, await FlexTemplate.MemorandumFlexMessageTemplate(userMemoes));
    }

    /// <summary>
    /// 新增備註
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public async Task<Memorandum> AddMemo(string msg, int userId)
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
    public async Task RemoveMemo(int memoId, int userId)
    {
        var memo = await memorandumRepository.GetMemorandum(memoId, userId);
        if (memo == null)
            return;
        memorandumRepository.Remove(memo);

        await unitOfWork.CompleteAsync();
    }

}
