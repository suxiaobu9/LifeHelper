using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(@"./data/wake_it_up-.log", rollingInterval: RollingInterval.Day, shared: true)
    .CreateLogger();

int delayMin = 10;
string? targetAddress = Environment.GetEnvironmentVariable("WAKEITUP_ADDRESS");
string? envDelayMin = Environment.GetEnvironmentVariable("DELAY_MIN");

if (!string.IsNullOrWhiteSpace(envDelayMin))
{
    var success = int.TryParse(envDelayMin, out var min);
    if (success)
        delayMin = min;
}

if (string.IsNullOrWhiteSpace(targetAddress))
{
    Log.Error($"{nameof(targetAddress)} can't be null or empty or whitespace");
    throw new Exception($"{nameof(targetAddress)} can't be null or empty or whitespace");
}

while (true)
{
    using HttpClient client = new();
    using HttpResponseMessage response = await client.PostAsync(targetAddress,new StringContent(""));

    // 檢查回應狀態碼
    if (response.IsSuccessStatusCode)
    {
        Log.Information("Wake up successfully.");
    }
    else
    {
        var content = await response.Content.ReadAsStringAsync();
        Log.Warning("Failed to wake up target address. {content}", content);
    }

    await Task.Delay(TimeSpan.FromMinutes(delayMin));
}
