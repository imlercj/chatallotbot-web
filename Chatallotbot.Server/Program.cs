using Chatallotbot.Server.Extensions;

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

app.UseHttpsRedirection();
app.UseRouting(); // Add this
app.MapControllers(); // Add this

app.Run();