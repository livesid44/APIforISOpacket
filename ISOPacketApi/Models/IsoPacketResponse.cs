namespace ISOPacketApi.Models;

public class IsoFieldDetail
{
    public int FieldNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Length { get; set; }
}

public class IsoPacketResponse
{
    public string MessageTypeIndicator { get; set; } = string.Empty;
    public string PrimaryBitmapHex { get; set; } = string.Empty;
    public string SecondaryBitmapHex { get; set; } = string.Empty;
    public List<IsoFieldDetail> Fields { get; set; } = new();
    public string PacketHex { get; set; } = string.Empty;
    public string PacketAscii { get; set; } = string.Empty;
    public int PacketLengthBytes { get; set; }
}
