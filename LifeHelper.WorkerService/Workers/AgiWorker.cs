using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LifeHelper.WorkerService.Workers;

public class AgiWorker : BackgroundService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<AgiWorker> logger;
    private readonly string hostAddress;
    private readonly int delayMin;

    public AgiWorker(HttpClient httpClient, IConfiguration configuration, ILogger<AgiWorker> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        hostAddress = configuration.GetSection("WakeUp:HostAddress").Value;
        delayMin = configuration.GetSection("WakeUp:DelayMinutes").Get<int>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                await (await httpClient.GetAsync(hostAddress, stoppingToken)).Content.ReadAsStringAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "");
            }
            await Task.Delay(TimeSpan.FromMinutes(delayMin), stoppingToken);
        }
    }
}
