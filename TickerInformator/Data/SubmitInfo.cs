using Newtonsoft.Json;

public partial class SubmitInfo
{
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("alertThreshold")]
    public int? AlertThreshold { get; set; }
}