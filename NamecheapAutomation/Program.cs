using NamecheapAutomation;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        int returnCode = 0;

        Console.CancelKeyPress += (_, _) => Environment.Exit(0);
        Console.OutputEncoding = Encoding.UTF8;

        AnsiConsole.Console
            .Write(new FigletText("NCAM")
            .Color(Color.Red3_1));

        var rootCommand = new RootCommand("Namecheap API Manager");
        var hostCommand = new Command("host", "Manage hosts for a domain.");

        hostCommand.AddOption(HostCommandHandler.Domain);
        hostCommand.AddOption(HostCommandHandler.Username);
        hostCommand.AddOption(HostCommandHandler.ApiKey);
        hostCommand.AddOption(HostCommandHandler.Sandbox);

        hostCommand.SetHandler(async (context) =>
        {
            returnCode = await new HostCommandHandler().InvokeAsync(context);
        });

        rootCommand.AddCommand(hostCommand);

        await rootCommand.InvokeAsync(args);

        return returnCode;
     }
}
