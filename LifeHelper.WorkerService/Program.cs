using LifeHelper.WorkerService.Workers;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging(configure =>
        {
            var seqLogAdd = hostContext.Configuration.GetSection("Seq:ServerAddress").Value;
            var seqLogApiKey = hostContext.Configuration.GetSection("Seq:ApiKey").Value;
            configure.AddSerilog(new LoggerConfiguration()
                    .WriteTo.Seq(seqLogAdd, apiKey: seqLogApiKey)
                    .WriteTo.Console()
                    .CreateLogger());
        });

        services.AddHttpClient(WorkerServiceParams.DucHttpClientName, option =>
        {
            var username = hostContext.Configuration.GetSection("duc:username").Value;
            var password = hostContext.Configuration.GetSection("duc:password").Value;

            option.DefaultRequestHeaders.Add("X-Auth-Email", username);
            option.DefaultRequestHeaders.Add("X-Auth-Key", password);
        });
        services.AddHttpClient();

        services.AddHostedService<WakeLineBotUpWorker>();
        services.AddHostedService<DucWorker>();

    })
    .Build();

await host.RunAsync();
