using Md5Pwner.Database;
using Md5Pwner.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((x, y) => y
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"));

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<PwnedContext>();
builder.Services.AddSingleton<PwnedWsServer>();
builder.Services.AddTransient<PwnedWsSession>();
builder.Services.AddHostedService<PwnedWsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

var wsServer = app.Services.GetRequiredService<PwnedWsServer>();
wsServer.Start();
app.Run();
wsServer.Stop();
