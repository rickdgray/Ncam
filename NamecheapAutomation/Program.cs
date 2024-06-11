using NamecheapAutomation;
using Spectre.Console;

internal class Program
{
    private static async Task Main()
    {
        using var httpClient = new HttpClient();
        var ip = await httpClient.GetStringAsync("https://api.seeip.org");

        // sandbox
        var api = new NameCheapApi("rickdgray", "rickdgray", "", ip, true);

        // prod
        //var api = new NameCheapApi("rickdgray", "rickdgray", "", ip);

        var response = await api.GetHostsAsync("rickdgray", "com");
        var hosts = response.HostEntries;

        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow([
            new Text("Host Name", new Style(Color.Green, Color.Black)),
            new Text("Record Type", new Style(Color.Red, Color.Black)).RightJustified(),
            new Text("Address", new Style(Color.Blue, Color.Black))
        ]);

        foreach (var host in hosts)
        {
            grid.AddRow([
                new Text(host.HostName),
                new Text(Enum.GetName(typeof(RecordType), host.RecordType) ?? string.Empty).RightJustified(),
                new Text(host.Address)
            ]);
        }

        AnsiConsole.Write(grid);

        Console.WriteLine();

        var selectedHost = AnsiConsole.Prompt(
            new SelectionPrompt<HostEntry>()
                .Title("Select a host entry:")
                .EnableSearch()
                .PageSize(3)
                .MoreChoicesText("Scroll down to see more hosts...")
                .AddChoices(hosts)
                .UseConverter(h => $"{h.HostName} {Enum.GetName(typeof(RecordType), h.RecordType) ?? string.Empty} {h.Address}")
        );

        AnsiConsole.MarkupLine($"Selected host: [bold]{selectedHost.HostName}[/]");
    }
}
