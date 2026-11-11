var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/v1/health");

app.Run();

/// <summary>Integratsion testlar (WebApplicationFactory) uchun ochiq kirish nuqtasi.</summary>
public partial class Program;
