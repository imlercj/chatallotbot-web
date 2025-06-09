using Chatallotbot.ClientDemo.Components;
using Chatallotbot.ClientDemo.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
var services = builder.Services;
services.AddRazorComponents()
    .AddInteractiveServerComponents();

services.AddHttpClient<ChatApiClient>(client =>
    {
        var serverApiUrl = $"{configuration["Server:BaseUrl"]}{configuration["Server:ApiPath"]}";
        client.BaseAddress = new Uri($"{serverApiUrl}");
        client.Timeout = TimeSpan.FromSeconds(240);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();