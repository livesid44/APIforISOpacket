using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ISOPacketApi.Models;
using ISOPacketApi.Services;

namespace ISOPacketApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ISOPacketController : ControllerBase
{
    private readonly IIsoPacketService _isoPacketService;
    private readonly IsoConnectionSettings _connectionSettings;

    public ISOPacketController(
        IIsoPacketService isoPacketService,
        IOptions<IsoConnectionSettings> connectionSettings)
    {
        _isoPacketService = isoPacketService;
        _connectionSettings = connectionSettings.Value;
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

    /// <summary>
    /// Returns the active ISO system connection settings loaded from configuration.
    /// Use this endpoint to verify that the correct host, port, and timeout values
    /// are in effect for the running environment.
    /// Edit these values in appsettings.json (or appsettings.{Environment}.json)
    /// under the "IsoConnection" key, or supply them as environment variables
    /// prefixed with "IsoConnection__" (e.g. IsoConnection__Host=192.168.1.10).
    /// </summary>
    [HttpGet("connection")]
    [ProducesResponseType(typeof(IsoConnectionSettings), StatusCodes.Status200OK)]
    public IActionResult GetConnectionSettings()
    {
        return Ok(_connectionSettings);
    }
}
