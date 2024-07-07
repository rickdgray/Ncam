using Microsoft.Extensions.Options;
using Ncam.Commands;
using Ncam.Models;
using Ncam.Services;
using Spectre.Console;
using System.CommandLine;

namespace Ncam
{
    public class Ncam(IOptions<GlobalParameters> parameters,
        IAnsiConsole console,
        IIpService ipService,
        INamecheapService namecheapService)
    {
        public const string Name = "NCAM";

        public async Task<int> ExecuteAsync(string[] args)
        {
            console
                .Write(new FigletText(Name)
                .Centered()
                .Color(Color.Red3));

            var rootCommand = new RootCommand("Namecheap API Manager");
            rootCommand.AddGlobalOption(GlobalOptions.Domain);
            rootCommand.AddGlobalOption(GlobalOptions.Username);
            rootCommand.AddGlobalOption(GlobalOptions.ApiKey);
            rootCommand.AddGlobalOption(GlobalOptions.Sandbox);

            var hostCommand = new Command("host", "Manage hosts for a domain.");
            hostCommand.SetHandler(async (context) =>
            {
                await new HostCommandHandler(parameters, console, ipService, namecheapService).InvokeAsync(context);
            });

            var hostAddCommand = new Command("add", "Add hosts to a domain.");
            hostAddCommand.AddOption(HostOptions.Hostname);
            hostAddCommand.AddOption(HostOptions.RecordType);
            hostAddCommand.AddOption(HostOptions.Address);
            hostAddCommand.SetHandler(async (context) =>
            {
                await new HostAddCommandHandler(parameters, console, ipService, namecheapService).InvokeAsync(context);
            });

            hostCommand.AddCommand(hostAddCommand);

            rootCommand.AddCommand(hostCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
