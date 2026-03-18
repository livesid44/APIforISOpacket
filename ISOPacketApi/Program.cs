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
});

// Register the ISO 8583 packet service
builder.Services.AddScoped<IIsoPacketService, IsoPacketService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ISO 8583 Packet API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
});

app.UseAuthorization();
app.MapControllers();

app.Run();
