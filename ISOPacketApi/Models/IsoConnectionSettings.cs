using System.ComponentModel.DataAnnotations;

namespace ISOPacketApi.Models;

/// <summary>
/// Connection settings for the remote ISO 8583 host system.
/// Configure these values in appsettings.json under the "IsoConnection" section.
/// There is no auto-discovery in ISO 8583 — the host and port must always be
/// provided explicitly by the operator.
/// </summary>
public class IsoConnectionSettings
{
    /// <summary>Configuration section name used in appsettings.json.</summary>
    public const string SectionName = "IsoConnection";

    /// <summary>
    /// Hostname or IP address of the ISO 8583 server (e.g. "192.168.1.100" or "iso-host.bank.local").
    /// </summary>
    [Required]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// TCP port the ISO 8583 server listens on (commonly 8583, 9000, or a bank-specific port).
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 8583;

    /// <summary>
    /// Number of seconds to wait when opening the TCP connection before giving up.
    /// </summary>
    [Range(1, 300)]
    public int ConnectTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Number of seconds to wait for a response after sending a packet.
    /// </summary>
    [Range(1, 300)]
    public int ReadTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Interval in seconds between keep-alive / echo (MTI 0800) messages sent to the ISO host.
    /// Set to 0 to disable keep-alive messages.
    /// </summary>
    [Range(0, 3600)]
    public int KeepAliveIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Number of times to retry a failed connection before reporting an error.
    /// </summary>
    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Seconds to wait between consecutive connection retry attempts.
    /// </summary>
    [Range(1, 60)]
    public int RetryDelaySeconds { get; set; } = 5;
}
