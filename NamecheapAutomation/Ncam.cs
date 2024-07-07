using Microsoft.Extensions.Options;
using NamecheapAutomation.Commands;
using NamecheapAutomation.Models;
using NamecheapAutomation.Services;
using Spectre.Console;
using System.CommandLine;

namespace NamecheapAutomation
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
            hostCommand.AddOption(HostCommandHandler.Hostname);
            hostCommand.AddOption(HostCommandHandler.RecordType);
            hostCommand.AddOption(HostCommandHandler.Address);

            hostCommand.SetHandler(async (context) =>
            {
                await new HostCommandHandler(parameters, console, ipService, namecheapService).InvokeAsync(context);
            });

            rootCommand.AddCommand(hostCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
