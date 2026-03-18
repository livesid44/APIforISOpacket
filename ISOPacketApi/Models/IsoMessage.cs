namespace ISOPacketApi.Models;

public class IsoMessage
{
    public string MessageTypeIndicator { get; set; } = string.Empty;
    public Dictionary<int, string> Fields { get; set; } = new();
}
