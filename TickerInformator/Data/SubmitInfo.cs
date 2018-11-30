using Newtonsoft.Json;

public partial class SubmitInfo
{
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("alertTreshold")]
    public int? AlertTreshold { get; set; }
}