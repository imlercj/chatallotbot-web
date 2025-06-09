using Chatallotbot.Server.Extensions;
using Chatallotbot.Server.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCustomConfiguration(builder.Environment);

var services = builder.Services;
services
    .AddOpenApi()
    .AddCustomAuthorization()
    .AddCustomServices()
    .AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseRouting(); // Add this
app.MapControllers(); // Add this

app.Run();