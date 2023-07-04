namespace LifeHelper.WorkerService.Workers;

public class WakeLineBotUpWorker : BackgroundService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<WakeLineBotUpWorker> logger;
    private readonly string hostAddress;
    private readonly int delayMin;

    public WakeLineBotUpWorker(HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WakeLineBotUpWorker> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        hostAddress = configuration.GetSection("WakeUp:HostAddress").Value;
        delayMin = configuration.GetSection("WakeUp:DelayMinutes").Get<int>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Wake up host start.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await httpClient.PostAsync(hostAddress, new StringContent(""), stoppingToken);

                var content = await response.Content.ReadAsStringAsync(stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Wake up host success.");
                }
                else
                {
                    logger.LogInformation("Wake up host fail.{content}", content);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Wake up get exception.");
            }

            await Task.Delay(TimeSpan.FromMinutes(delayMin), stoppingToken);

        }

        logger.LogInformation("Wake up host end.");
    }
}
