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
    public async Task UploadBlobAsync(string blobName, string json, bool overwrite = false)
    {
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var blob = blobContainerClient.GetBlobClient(blobName);

        await blob.UploadAsync(memoryStream, overwrite: overwrite);
    }

    /// <summary>
    /// 更新 Blob
    /// </summary>
    /// <param name="blobName"></param>
    /// <param name="json"></param>
    /// <returns></returns>
    public async Task UpdateBlobAsync(string blobName, string json)
    {
        await UploadBlobAsync(blobName, json, true);
    }

    /// <summary>
    /// 取得 Blob 資料夾底下的資料
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetBlobs<T>(string prefix)
    {
        var blobs = blobContainerClient.GetBlobs(prefix: prefix).OrderBy(x=>x.Properties.CreatedOn).ToArray();

        var result = new T[blobs.Length];

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 5
        };

        await Parallel.ForEachAsync(Enumerable.Range(0, blobs.Length), parallelOptions, async (index, token) =>
        {
            using var stream = (await blobContainerClient.GetBlobClient(blobs[index].Name).DownloadStreamingAsync(cancellationToken: token)).Value.Content;
            var deserializeObj = JsonSerializer.Deserialize<T>(await new StreamReader(stream).ReadToEndAsync());
            if (deserializeObj != null)
                result[index] = deserializeObj;
        });

        return result.Where(x=>x != null);
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
    public DateTime[]? GetAllMonth(Guid userId)
    {
        var userAccountingDirectory = BlobConst.UserAccountingDirectory(userId);

        var months = blobContainerClient.GetBlobsByHierarchy(prefix: userAccountingDirectory, delimiter: "/")
            .Select(x =>
            {
                var date = x.Prefix.Replace(userAccountingDirectory, "") + "01";
                return DateTime.ParseExact(date, "yyyyMM/dd", null);
            }).ToArray();

        return months;
    }

    /// <summary>
    /// 刪除 Blob
    /// </summary>
    /// <param name="blobName"></param>
    /// <returns></returns>
    public async Task DeleteBlob(string blobName)
    {
        var blob = blobContainerClient.GetBlobClient(blobName);

        await blob.DeleteIfExistsAsync();
    }

}

