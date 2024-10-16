using LifeHelper.WorkerService.Model;
using System.Net.Http.Json;

namespace LifeHelper.WorkerService.Workers;

public class DucWorker : BackgroundService
{
    private readonly ILogger<DucWorker> logger;
    private readonly HttpClient httpClient;
    private readonly HttpClient ducHttpClient;
    private readonly int delayMin;
    private readonly string zoneId;
    private readonly string hostName;
    private string oldIp = "";

    public DucWorker(ILogger<DucWorker> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        HttpClient httpClient)
    {
        this.logger = logger;
        this.ducHttpClient = httpClientFactory.CreateClient(WorkerServiceParams.DucHttpClientName);
        this.httpClient = httpClient;

        delayMin = configuration.GetSection("Duc:DelayMinutes").Get<int>();

        zoneId = configuration.GetSection("Duc:ZoneId").Value;

        hostName = configuration.GetSection("Duc:HostNames").Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(hostName))
        {
            return;
        }

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
            var currentIpTask = GetPublicIpAddress();

            var getDnsRecordsApi = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records";

            var zonesResult = await ducHttpClient.GetFromJsonAsync<Zones>(getDnsRecordsApi);

            if (zonesResult?.Result == null)
            {
                logger.LogWarning("Dns records get null.");
                return;
            }

            var currentIp = await currentIpTask;

            if (!string.IsNullOrWhiteSpace(currentIp) && currentIp == oldIp)
            {
                logger.LogInformation("IP address unchanged.");
                return;
            }

            var hostNames = hostName.Split(",");

            if (hostNames == null || hostNames.Length == 0)
            {
                logger.LogWarning("Host name get null.");
                return;
            }

            List<Task<HttpResponseMessage>> tasks = new();

            foreach (var dnsRecord in zonesResult.Result)
            {
                if (!hostNames.Any(x => x == dnsRecord.Name))
                    continue;

                var updateDnsApi = $"https://api.cloudflare.com/client/v4/zones/{dnsRecord.ZoneId}/dns_records/{dnsRecord.Id}";

                var model = new UpdateDnsRecordModel
                {
                    Content = currentIp,
                    Name = dnsRecord.Name,
                    Proxied = dnsRecord.Proxied,
                    Ttl = dnsRecord.Ttl,
                    Type = dnsRecord.Type
                };

                tasks.Add(ducHttpClient.PutAsJsonAsync(updateDnsApi, model));
            }

            var responses = await Task.WhenAll(tasks);

            foreach (HttpResponseMessage response in responses)
            {
                if (response.IsSuccessStatusCode)
                    continue;

                string url = response.RequestMessage!.RequestUri!.ToString();

                var content = await response.Content.ReadAsStringAsync();

                logger.LogWarning("Update dns fail.{url} {content}", url, content);
            }

            oldIp = currentIp;

            logger.LogInformation("IP address changed to {ip}.", currentIp);

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
