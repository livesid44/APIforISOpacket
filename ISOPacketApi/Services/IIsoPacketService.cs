using ISOPacketApi.Models;

namespace ISOPacketApi.Services;

public interface IIsoPacketService
{
    IsoMessage BuildPredefinedMessage();
    IsoPacketResponse BuildPacket(IsoMessage message);
}
