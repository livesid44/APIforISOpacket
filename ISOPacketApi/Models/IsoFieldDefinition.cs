namespace ISOPacketApi.Models;

public enum IsoFieldType
{
    Fixed,
    LLVar,  // Length prefix is 2 digits
    LLLVar  // Length prefix is 3 digits
}

public class IsoFieldDefinition
{
    public int FieldNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public IsoFieldType Type { get; set; }
    public int MaxLength { get; set; }
}
