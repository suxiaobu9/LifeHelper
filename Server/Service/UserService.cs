using LifeHelper.Shared.Models.LIFF;

namespace LifeHelper.Server.Service;

public class UserService
{
    private readonly UserRepository userRepository;
    private readonly UnitOfWork unitOfWork;
    public UserService(UserRepository userRepository,
        UnitOfWork unitOfWork)
    {
        this.userRepository = userRepository;
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 取得使用者
    /// </summary>
    /// <param name="userLineIds"></param>
    /// <returns></returns>
    public async Task<User[]> GetUsers(string[] userLineIds)
    {
        return await userRepository.GetUsers(userLineIds);
    }

    /// <summary>
    /// 新增使用者
    /// </summary>
    /// <param name="userLineId"></param>
    /// <returns></returns>
    public async Task<User> AddUser(string userLineId)
    {
        var user = new User
        {
            LineUserId = userLineId,
            Name = userLineId,
            IsAdmin = false
        };

        await userRepository.AddAsync(user);

        await unitOfWork.CompleteAsync();

        return user;
    }

    /// <summary>
    /// Upsert 目前使用者 並回傳
    /// </summary>
    /// <returns></returns>
    public async Task<User?> UpsertUser(UserProfile userProfile)
    {
        var user = await userRepository.GetUser(userProfile.UserLineId);

        if (user == null)
        {
            user = new User
            {
                IsAdmin = false,
                LineUserId = userProfile.UserLineId,
            };

            await userRepository.AddAsync(user);
        }

        user.Name = userProfile.Name;

        await unitOfWork.CompleteAsync();

        return user;
    }
}
