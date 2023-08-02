using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LifeHelper.WorkerService.Model;

public class Zones
{
    [JsonPropertyName("result")]
    public Result[]? Result { get; set; }
}


public class Result
{

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("zone_id")]
    public string? ZoneId { get; set; }

    [JsonPropertyName("zone_name")]
    public string? ZoneName { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; }

    [JsonPropertyName("ttl")]
    public int Ttl { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
