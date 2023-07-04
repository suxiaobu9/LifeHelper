namespace LifeHelper.WorkerService.Workers;

public class DucWorker : BackgroundService
{
    private readonly ILogger<DucWorker> logger;
    private readonly HttpClient httpClient;
    private readonly HttpClient noIpHttpClient;
    private readonly int delayMin;
    private readonly string hostName;
    private string oldIp = "";

    public DucWorker(ILogger<DucWorker> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        HttpClient httpClient)
    {
        this.logger = logger;
        this.noIpHttpClient = httpClientFactory.CreateClient(WorkerServicePatams.NoIpHttpClientName);
        this.httpClient = httpClient;

        delayMin = configuration.GetSection("Duc:DelayMinutes").Get<int>();

        hostName = configuration.GetSection("Duc:HostNames").Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Duc start.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ChangeDnsIp();

            await Task.Delay(TimeSpan.FromMinutes(delayMin), stoppingToken);
        }

        logger.LogInformation("Duc end.");
    }

    private async Task ChangeDnsIp()
    {
        try
        {
            // 取得目前的公開IP位址
            string currentIp = await GetPublicIpAddress();

            if (!string.IsNullOrWhiteSpace(currentIp) && currentIp == oldIp)
            {
                logger.LogInformation("IP address unchanged.");
                return;
            }

            // 構建更新No-IP域名IP的URL
            string updateUrl = $"https://dynupdate.no-ip.com/nic/update?hostname={hostName}&myip={currentIp}";

            // 發送HTTP GET請求以更新IP
            using HttpResponseMessage response = await noIpHttpClient.GetAsync(updateUrl);

            if (response.IsSuccessStatusCode)
            {
                oldIp = currentIp;
                logger.LogInformation("IP address updated successfully.{IP}", currentIp);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Failed to update IP address. {content}", content);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Duc get exception.");
        }
    }

    private async Task<string> GetPublicIpAddress()
    {
        var apiUrls = new string[]
        {
            "https://checkip.amazonaws.com",
            "https://ifconfig.co/ip",
            "https://ipinfo.io/ip"
        };

        foreach (var url in apiUrls)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Get ip error.{content}", content);
                continue;
            }

            return content.Trim();
        }

        throw new Exception("Failed to retrieve public IP address.");
    }
}
