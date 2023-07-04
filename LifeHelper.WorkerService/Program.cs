using LifeHelper.WorkerService.Workers;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging(configure =>
        {
            var seqLogAdd = hostContext.Configuration.GetSection("SeqLogServerAddress").Value;
            configure.AddSerilog(new LoggerConfiguration()
                    .WriteTo.Seq(seqLogAdd)
                    .WriteTo.Console()
                    .CreateLogger());
        });

        services.AddHttpClient(WorkerServicePatams.NoIpHttpClientName, option =>
        {
            var username = hostContext.Configuration.GetSection("duc:username").Value;
            var password = hostContext.Configuration.GetSection("duc:password").Value;
            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            option.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        });
        services.AddHttpClient();

        services.AddHostedService<WakeLineBotUpWorker>();
        services.AddHostedService<DucWorker>();
    
    })
    .Build();

await host.RunAsync();
