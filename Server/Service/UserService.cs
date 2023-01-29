using LifeHelper.Shared.Const;
using LifeHelper.Shared.Models.LIFF;

namespace LifeHelper.Server.Service;

public class UserService : IUserService
{
    private readonly AzureBlobStorageService azureBlobStorageService;

    public UserService(AzureBlobStorageService azureBlobStorageService)
    {
        this.azureBlobStorageService = azureBlobStorageService;
    }

    /// <summary>
    /// 新增使用者
    /// </summary>
    /// <param name="userLineId"></param>
    /// <returns></returns>
    public async Task<User> AddUserAsync(string userLineId)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            LineUserId = userLineId,
            Name = userLineId[..10],
            IsAdmin = false
        };

        await azureBlobStorageService.UploadBlobAsync(BlobConst.UserBlobName(userLineId), JsonSerializer.Serialize(user));

        return user;

    }

    /// <summary>
    /// 取得使用者
    /// </summary>
    /// <param name="userLineIds"></param>
    /// <returns></returns>
    public async Task<User?> GetUserAsync(string userLineId)
    {
        return await azureBlobStorageService.GetBlob<User>(BlobConst.UserBlobName(userLineId));
    }

    /// <summary>
    /// Upsert 目前使用者 並回傳
    /// </summary>
    /// <param name="userProfile"></param>
    /// <returns></returns>
    public async Task<User?> UpsertUserAsync(UserProfile userProfile)
    {
        var user = await azureBlobStorageService.GetBlob<User>(BlobConst.UserBlobName(userProfile.UserLineId));

        if (user == null)
            return user;

        if (user.Name == userProfile.Name)
            return user;

        user.Name = userProfile.Name;

        await azureBlobStorageService.UpdateBlobAsync(BlobConst.UserBlobName(userProfile.UserLineId), JsonSerializer.Serialize(user));

        return user;
    }
}
