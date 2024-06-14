using NamecheapAutomation;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.CancelKeyPress += (_, _) => Environment.Exit(0);
        Console.OutputEncoding = Encoding.UTF8;

        AnsiConsole.Console
            .Write(new FigletText("NCAM")
            .Color(Color.Red3_1));

        var domain = new Option<string>("--domain", "The domain to manage DNS records on.")
        {
            IsRequired = true
        };
        domain.AddAlias("-d");

        var username = new Option<string>("--username", "The Namecheap username.")
        {
            IsRequired = true
        };
        username.AddAlias("-u");

        var apiKey = new Option<string>("--apiKey", "The Namecheap API key.")
        {
            IsRequired = true
        };
        apiKey.AddAlias("-k");

        var sandbox = new Option<bool>("--sandbox", "Use the Namecheap sandbox API.")
        {
            IsRequired = false
        };
        sandbox.AddAlias("-s");

        var rootCommand = new RootCommand("Namecheap API Manager");
        var hostCommand = new Command("host", "Manage hosts for a domain.");

        hostCommand.AddOption(domain);
        hostCommand.AddOption(username);
        hostCommand.AddOption(apiKey);
        hostCommand.AddOption(sandbox);

        hostCommand.SetHandler(HostCommandHandler.Handle, domain, username, apiKey, sandbox);

        rootCommand.AddCommand(hostCommand);

        await rootCommand.InvokeAsync(args);
     }
}
