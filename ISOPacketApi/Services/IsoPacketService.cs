using System.Text;
using ISOPacketApi.Models;

namespace ISOPacketApi.Services;

public class IsoPacketService : IIsoPacketService
{
    // ISO 8583 field definitions (subset relevant to this implementation)
    private static readonly Dictionary<int, IsoFieldDefinition> FieldDefinitions = new()
    {
        { 3,   new IsoFieldDefinition { FieldNumber = 3,   Name = "Processing Code",                           Type = IsoFieldType.Fixed,  MaxLength = 6  } },
        { 4,   new IsoFieldDefinition { FieldNumber = 4,   Name = "Amount, Transaction",                      Type = IsoFieldType.Fixed,  MaxLength = 16 } },
        { 11,  new IsoFieldDefinition { FieldNumber = 11,  Name = "Systems Trace Audit Number",               Type = IsoFieldType.Fixed,  MaxLength = 12 } },
        { 12,  new IsoFieldDefinition { FieldNumber = 12,  Name = "Date and Time, Local Transaction",         Type = IsoFieldType.Fixed,  MaxLength = 14 } },
        { 17,  new IsoFieldDefinition { FieldNumber = 17,  Name = "Date, Capture",                            Type = IsoFieldType.Fixed,  MaxLength = 8  } },
        { 24,  new IsoFieldDefinition { FieldNumber = 24,  Name = "Network International Identifier",         Type = IsoFieldType.Fixed,  MaxLength = 3  } },
        { 32,  new IsoFieldDefinition { FieldNumber = 32,  Name = "Acquiring Institution Identification Code",Type = IsoFieldType.LLVar,  MaxLength = 11 } },
        { 34,  new IsoFieldDefinition { FieldNumber = 34,  Name = "Primary Account Number, Extended",        Type = IsoFieldType.LLVar,  MaxLength = 28 } },
        { 41,  new IsoFieldDefinition { FieldNumber = 41,  Name = "Card Acceptor Terminal Identification",   Type = IsoFieldType.Fixed,  MaxLength = 16 } },
        { 43,  new IsoFieldDefinition { FieldNumber = 43,  Name = "Card Acceptor Name/Location",             Type = IsoFieldType.LLVar,  MaxLength = 40 } },
        { 49,  new IsoFieldDefinition { FieldNumber = 49,  Name = "Currency Code, Transaction",               Type = IsoFieldType.Fixed,  MaxLength = 3  } },
        { 102, new IsoFieldDefinition { FieldNumber = 102, Name = "Account Identification 1",                 Type = IsoFieldType.LLVar,  MaxLength = 28 } },
        { 123, new IsoFieldDefinition { FieldNumber = 123, Name = "Reserved National (Field 123)",            Type = IsoFieldType.LLLVar, MaxLength = 999 } },
        { 125, new IsoFieldDefinition { FieldNumber = 125, Name = "Reserved National (Field 125)",            Type = IsoFieldType.LLLVar, MaxLength = 999 } },
        { 126, new IsoFieldDefinition { FieldNumber = 126, Name = "Reserved National (Field 126)",            Type = IsoFieldType.LLLVar, MaxLength = 999 } },
    };

    /// <summary>
    /// Builds the predefined ISO 8583 message with all required fields.
    /// </summary>
    public IsoMessage BuildPredefinedMessage()
    {
        return new IsoMessage
        {
            MessageTypeIndicator = "1200",
            Fields = new Dictionary<int, string>
            {
                { 3,   "970000" },
                { 4,   "0000000000000000" },
                { 11,  "910017945323" },
                { 12,  "20190410171832" },
                { 17,  "20190410" },
                { 24,  "200" },
                { 32,  "012" },
                { 34,  "17000000" },
                { 41,  "00000000000000KO" },
                { 43,  "0" },
                { 49,  "INR" },
                { 102, "012        0000    0000000000000000" },
                { 123, "TLC" },
                { 125, "CPPS-DATA" },
                { 126, "31680100020711|000013|12-03-2026|50000000|31|BANK OF BARODA||Kshama Gupta||3168010002071100001312032026||||TLC|" },
            }
        };
    }

    /// <summary>
    /// Builds an ISO 8583 packet from the given message, computing the bitmap and
    /// encoding each data element according to its field definition.
    /// </summary>
    public IsoPacketResponse BuildPacket(IsoMessage message)
    {
        var sortedFields = message.Fields.Keys.OrderBy(k => k).ToList();

        // Compute primary and secondary bitmaps (64-bit each)
        var primaryBitmap = new byte[8];
        var secondaryBitmap = new byte[8];
        bool hasSecondaryFields = sortedFields.Any(f => f > 64);

        if (hasSecondaryFields)
        {
            // Bit 1 of primary bitmap signals secondary bitmap presence
            SetBitmapBit(primaryBitmap, 1);
        }

        foreach (var fieldNumber in sortedFields)
        {
            if (fieldNumber <= 64)
            {
                SetBitmapBit(primaryBitmap, fieldNumber);
            }
            else
            {
                SetBitmapBit(secondaryBitmap, fieldNumber - 64);
            }
        }

        // Build data element bytes
        var dataBytes = new List<byte>();
        var fieldDetails = new List<IsoFieldDetail>();

        foreach (var fieldNumber in sortedFields)
        {
            var value = message.Fields[fieldNumber];
            var def = FieldDefinitions.TryGetValue(fieldNumber, out var d)
                ? d
                : new IsoFieldDefinition { FieldNumber = fieldNumber, Name = $"Field {fieldNumber}", Type = IsoFieldType.Fixed, MaxLength = value.Length };

            var encodedBytes = EncodeField(def, value);
            dataBytes.AddRange(encodedBytes);

            fieldDetails.Add(new IsoFieldDetail
            {
                FieldNumber = fieldNumber,
                Name = def.Name,
                Value = value,
                Length = value.Length
            });
        }

        // Assemble full packet: MTI + primary bitmap + [secondary bitmap] + data
        var packetBytes = new List<byte>();

        // MTI as ASCII bytes
        packetBytes.AddRange(Encoding.ASCII.GetBytes(message.MessageTypeIndicator));

        // Primary bitmap
        packetBytes.AddRange(primaryBitmap);

        // Secondary bitmap (if fields > 64 are present)
        if (hasSecondaryFields)
        {
            packetBytes.AddRange(secondaryBitmap);
        }

        // Data elements
        packetBytes.AddRange(dataBytes);

        var packet = packetBytes.ToArray();

        return new IsoPacketResponse
        {
            MessageTypeIndicator = message.MessageTypeIndicator,
            PrimaryBitmapHex = BitConverter.ToString(primaryBitmap).Replace("-", " "),
            SecondaryBitmapHex = hasSecondaryFields
                ? BitConverter.ToString(secondaryBitmap).Replace("-", " ")
                : string.Empty,
            Fields = fieldDetails,
            PacketHex = BitConverter.ToString(packet).Replace("-", " "),
            PacketAscii = ToSafeAscii(packet),
            PacketLengthBytes = packet.Length
        };
    }

    // Sets the bit at 'position' (1-indexed) in the bitmap byte array.
    private static void SetBitmapBit(byte[] bitmap, int position)
    {
        int byteIndex = (position - 1) / 8;
        int bitIndex = 7 - ((position - 1) % 8);
        bitmap[byteIndex] |= (byte)(1 << bitIndex);
    }

    // Encodes a field value according to its type (fixed, LL-VAR, LLL-VAR).
    private static byte[] EncodeField(IsoFieldDefinition def, string value)
    {
        return def.Type switch
        {
            IsoFieldType.LLVar  => Encoding.ASCII.GetBytes($"{value.Length:D2}{value}"),
            IsoFieldType.LLLVar => Encoding.ASCII.GetBytes($"{value.Length:D3}{value}"),
            _                   => Encoding.ASCII.GetBytes(value)
        };
    }

    // Returns an ASCII-safe string, replacing non-printable bytes with '.'.
    private static string ToSafeAscii(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length);
        foreach (var b in bytes)
        {
            sb.Append(b >= 0x20 && b <= 0x7E ? (char)b : '.');
        }
        return sb.ToString();
    }
}
