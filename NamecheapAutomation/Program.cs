using NamecheapAutomation;
using Spectre.Console;

internal class Program
{
    private static async Task Main()
    {
        var selectedOperation = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select an operation:")
                .PageSize(10)
                .AddChoices([
                    "View current hosts",
                    "Add a host",
                    "Update a host",
                    "Delete a host"
                ])
        );

        using var httpClient = new HttpClient();
        var ip = await httpClient.GetStringAsync("https://api.seeip.org");

        // sandbox
        var api = new NamecheapApi("rickdgray", "97f39e43010f4bd8bc6fb9427263139c", ip, true);

        // prod
        //var api = new NameCheapApi("rickdgray", "", ip);

        var response = await api.GetHostsAsync("rickdgray.com");
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
                new Text(host.Hostname),
                new Text(Enum.GetName(typeof(RecordType), host.RecordType) ?? string.Empty).RightJustified(),
                new Text(host.Address)
            ]);
        }

        AnsiConsole.Write(grid);

        if (selectedOperation == "View current hosts")
        {
            return;
        }

        Console.WriteLine();

        if (selectedOperation == "Add a host")
        {
            var hostname = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the host name:")
            );

            var recordType = AnsiConsole.Prompt(
                new SelectionPrompt<RecordType>()
                    .Title("Select a record type:")
                    .PageSize(3)
                    .AddChoices(Enum.GetValues<RecordType>())
            );

            var address = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the address [[IP]]:")
                    .AllowEmpty()
            );

            if (address == string.Empty)
            {
                address = ip;
            }

            // TODO: is this in place? maybe switch to a list
            hosts.Append(new HostEntry
            {
                Hostname = hostname,
                RecordType = recordType,
                Address = address
            });

            await api.SetHostsAsync("rickdgray.com", hosts);

            return;
        }

        var selectedHost = AnsiConsole.Prompt(
            new SelectionPrompt<HostEntry>()
                .Title("Select a host entry:")
                .EnableSearch()
                .PageSize(3)
                .MoreChoicesText("Scroll down to see more hosts...")
                .AddChoices(hosts)
                .UseConverter(h => $"{h.Hostname} {Enum.GetName(typeof(RecordType), h.RecordType) ?? string.Empty} {h.Address}")
        );

        AnsiConsole.MarkupLine($"Selected host: [bold]{selectedHost.Hostname}[/]");

        if (selectedOperation == "Update a host")
        {
            selectedHost.Hostname = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the host name:")
            );

            selectedHost.RecordType = AnsiConsole.Prompt(
                new SelectionPrompt<RecordType>()
                    .Title("Select a record type:")
                    .PageSize(3)
                    .AddChoices(Enum.GetValues<RecordType>())
            );

            selectedHost.Address = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the address [[IP]]:")
                    .AllowEmpty()
            );

            if (selectedHost.Address == string.Empty)
            {
                selectedHost.Address = ip;
            }

            await api.SetHostsAsync("rickdgray.com", hosts);

            return;
        }

        // TODO: remove the selected host

        await api.SetHostsAsync("rickdgray.com", hosts);
     }
}
