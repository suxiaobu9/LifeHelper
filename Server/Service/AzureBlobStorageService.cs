using Azure.Storage;
using Azure.Storage.Blobs;
using LifeHelper.Shared.Const;
using LifeHelper.Shared.Models.AppSettings;
using Microsoft.Extensions.Options;
using System.Text;

namespace LifeHelper.Server.Service;

public class AzureBlobStorageService
{
    private readonly AzureBlobStorage azureBlobStorageSettings;
    private readonly BlobContainerClient blobContainerClient;

    public AzureBlobStorageService(IOptions<AzureBlobStorage> azureBlobStorageSettings)
    {
        this.azureBlobStorageSettings = azureBlobStorageSettings.Value;
        this.blobContainerClient = GetBlobContainerClient();
    }

    private BlobContainerClient GetBlobContainerClient()
    {
        StorageSharedKeyCredential sharedKeyCredential =
            new(azureBlobStorageSettings.AccountName, azureBlobStorageSettings.AccountKey);

        string blobUri = $"https://{azureBlobStorageSettings.AccountName}.blob.core.windows.net";

        var blobServiceClient = new BlobServiceClient(new Uri(blobUri), sharedKeyCredential);

        if (string.IsNullOrWhiteSpace(azureBlobStorageSettings.BlobContainerName))
            throw new Exception(nameof(azureBlobStorageSettings.BlobContainerName) + " can't be null or whitespace");

        var container = blobServiceClient.GetBlobContainerClient(azureBlobStorageSettings.BlobContainerName);

        container.CreateIfNotExists();

        return container;
    }

    /// <summary>
    /// 上傳文字檔案
    /// </summary>
    /// <param name="blobName"></param>
    /// <param name="json"></param>
    /// <returns></returns>
    public async Task UploadBlobAsync(string blobName, string json)
    {
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var blob = blobContainerClient.GetBlobClient(blobName);

        await blob.UploadAsync(memoryStream, overwrite: true);
    }

    public async Task RemoveAccountingAsync(string blobName)
    {
        var blob = blobContainerClient.GetBlobClient(blobName);

        await blob.DeleteIfExistsAsync();
    }

    /// <summary>
    /// 取得 Blob 資料夾底下的資料
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetBlobs<T>(string prefix)
    {
        var result = new List<T>();

        var blobs = blobContainerClient.GetBlobsAsync(prefix: prefix);

        await foreach (var item in blobs)
        {
            using var stream = (await blobContainerClient.GetBlobClient(item.Name).DownloadStreamingAsync()).Value.Content;

            var deserializeObj = JsonSerializer.Deserialize<T>(await new StreamReader(stream).ReadToEndAsync());

            if (deserializeObj != null)
                result.Add(deserializeObj);
        }

        return result;
    }

    /// <summary>
    /// 取得單一 Blob
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="blobName"></param>
    /// <returns></returns>
    public async Task<T?> GetBlob<T>(string blobName)
    {
        var blob = blobContainerClient.GetBlobClient(blobName);

        if (await blob.ExistsAsync())
        {
            using var stream = (await blobContainerClient.GetBlobClient(blobName).DownloadStreamingAsync()).Value.Content;

            return JsonSerializer.Deserialize<T>(await new StreamReader(stream).ReadToEndAsync());
        }

        return default;
    }

    /// <summary>
    /// 取得所有有記帳的年月
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<DateTime?>> GetAllMonth(Guid userId)
    {
        var userAccountingDirectory = BlobConst.UserAccountingDirectory(userId);

        var allBlobss = blobContainerClient.GetBlobsByHierarchyAsync(prefix: userAccountingDirectory, delimiter: "/");

        var months = new List<DateTime?>();

        await foreach (var item in allBlobss)
        {
            var date = item.Prefix.Replace(userAccountingDirectory, "") + "01";

            months.Add(DateTime.ParseExact(date, "yyyyMM/dd", null));
        }

        return months;
    }

    public async Task DeleteBlob(string blobName)
    {
        var blob = blobContainerClient.GetBlobClient(blobName);

        await blob.DeleteIfExistsAsync();
    }

}

