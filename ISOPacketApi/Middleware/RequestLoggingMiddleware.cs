using System.Diagnostics;

namespace ISOPacketApi.Middleware;

/// <summary>
/// ASP.NET Core middleware that logs every HTTP request as a single line in a TXT file.
/// Log path is read from the "Logging:FilePath" configuration key; defaults to "logs/api-hits.txt".
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly string _logFilePath;
    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _logFilePath = configuration["Logging:FilePath"] ?? "logs/api-hits.txt";

        // Ensure the log directory exists
        var dir = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await _next(context);

        sw.Stop();

        var entry = FormatLogEntry(context, sw.ElapsedMilliseconds);

        // Write to TXT log file (thread-safe)
        await _fileLock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(_logFilePath, entry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write to the TXT log file at {Path}", _logFilePath);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private static string FormatLogEntry(HttpContext context, long elapsedMs)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + " UTC";
        var method    = context.Request.Method;
        var path      = context.Request.Path.Value ?? "/";
        var query     = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
        var status    = context.Response.StatusCode;
        var ip        = context.Connection.RemoteIpAddress?.ToString() ?? "-";

        return $"[{timestamp}] {method} {path}{query} | Status: {status} | Duration: {elapsedMs}ms | IP: {ip}";
    }
}
