using Microsoft.Extensions.Options;
using NamecheapAutomation.Commands;
using NamecheapAutomation.Services;
using Spectre.Console;
using System.CommandLine;

namespace NamecheapAutomation
{
    public class Ncam(IOptions<GlobalParameters> parameters,
        IIpService ipService,
        INamecheapService namecheapService)
    {
        public async Task<int> ExecuteAsync(string[] args)
        {
            AnsiConsole.Console
                .Write(new FigletText("NCAM")
                .Color(Color.Red3_1));

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
                await new HostCommandHandler(parameters, ipService, namecheapService).InvokeAsync(context);
            });

            rootCommand.AddCommand(hostCommand);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
