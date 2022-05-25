using System.Text.Json.Serialization;

namespace LifeHelper.Shared.Models.LIFF;

public class UserProfile
{
    [JsonPropertyName("sub")]
    public string UserLineId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
