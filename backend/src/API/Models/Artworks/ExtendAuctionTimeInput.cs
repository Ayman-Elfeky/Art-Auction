using System.Text.Json.Serialization;

namespace Api.Models;

/// <summary>Payload used to extend auction end time</summary>
public class ExtendAuctionTimeInput
{
    [JsonPropertyName("newEndTime")]
    public DateTime NewEndTime { get; set; }

    public ExtendAuctionTimeInput() { }

    public ExtendAuctionTimeInput(DateTime newEndTime)
    {
        NewEndTime = newEndTime;
    }
}
