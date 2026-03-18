using ISOPacketApi.Middleware;
using ISOPacketApi.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ISO 8583 Packet API",
        Version = "v1",
        Description = "A .NET API that builds and encodes ISO 8583 financial transaction packets."
    });

    // Include XML doc comments in Swagger (works for both Debug and published builds)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Register the ISO 8583 packet service
builder.Services.AddScoped<IIsoPacketService, IsoPacketService>();

var app = builder.Build();

// Log every API hit to a TXT file — registered first to capture all requests
app.UseMiddleware<RequestLoggingMiddleware>();

// Swagger is intentionally enabled in all environments (including production)
// so that the API can be tested after publishing.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISO 8583 Packet API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at the root "/"
});

app.UseAuthorization();
app.MapControllers();

app.Run();
