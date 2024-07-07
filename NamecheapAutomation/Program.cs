using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NamecheapAutomation;
using NamecheapAutomation.Services;
using Spectre.Console;
using System.Text;

Console.CancelKeyPress += (_, _) => Environment.Exit(0);
Console.OutputEncoding = Encoding.UTF8;

var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
builder.Logging.ClearProviders();
builder.Services.AddOptions<GlobalParameters>();
builder.Services.AddHttpClient<IIpService, IpService>();
builder.Services.AddHttpClient<INamecheapService, NamecheapService>();
builder.Services.AddSingleton(AnsiConsole.Console);
builder.Services.AddTransient<Ncam>();

var host = builder.Build();
await host.Services
    .GetRequiredService<Ncam>()
    .ExecuteAsync(args);
