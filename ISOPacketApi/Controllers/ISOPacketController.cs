using Microsoft.AspNetCore.Mvc;
using ISOPacketApi.Models;
using ISOPacketApi.Services;

namespace ISOPacketApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ISOPacketController : ControllerBase
{
    private readonly IIsoPacketService _isoPacketService;

    public ISOPacketController(IIsoPacketService isoPacketService)
    {
        _isoPacketService = isoPacketService;
    }

    /// <summary>
    /// Returns the predefined ISO 8583 (MTI 1200) packet with all required fields,
    /// including the raw packet hex, bitmap breakdown, and per-field details.
    /// </summary>
    [HttpGet("build")]
    [ProducesResponseType(typeof(IsoPacketResponse), StatusCodes.Status200OK)]
    public IActionResult BuildPacket()
    {
        var message = _isoPacketService.BuildPredefinedMessage();
        var response = _isoPacketService.BuildPacket(message);
        return Ok(response);
    }

    /// <summary>
    /// Accepts a custom ISO 8583 message (MTI + field map) and returns the encoded packet.
    /// </summary>
    [HttpPost("build")]
    [ProducesResponseType(typeof(IsoPacketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult BuildCustomPacket([FromBody] IsoMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.MessageTypeIndicator) ||
            message.MessageTypeIndicator.Length != 4)
        {
            return BadRequest("MessageTypeIndicator must be exactly 4 digits (e.g. \"1200\").");
        }

        if (message.Fields == null || message.Fields.Count == 0)
        {
            return BadRequest("At least one field must be provided.");
        }

        var response = _isoPacketService.BuildPacket(message);
        return Ok(response);
    }
}
