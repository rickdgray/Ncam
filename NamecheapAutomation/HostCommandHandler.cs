using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NamecheapAutomation
{
    public class HostCommandHandler : ICommandHandler
    {
        public static readonly Option<string> Domain = new(["--domain", "-d"], "The domain to manage DNS records on.")
        {
            IsRequired = true
        };

        public static readonly Option<string> Username = new(["--username", "-u"], "The Namecheap username.")
        {
            IsRequired = true
        };

        public static readonly Option<string> ApiKey = new(["--apiKey", "-k"], "The Namecheap API key.")
        {
            IsRequired = true
        };

        public static readonly Option<bool> Sandbox = new(["--sandbox", "-s"], "Use the Namecheap sandbox API.")
        {
            IsRequired = false
        };

        public int Invoke(InvocationContext context)
        {
            throw new NotImplementedException();
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var cancellationToken = context.GetCancellationToken();

            var domain = context.ParseResult.GetValueForOption(Domain);
            var username = context.ParseResult.GetValueForOption(Username);
            var apiKey = context.ParseResult.GetValueForOption(ApiKey);
            var sandbox = context.ParseResult.GetValueForOption(Sandbox);

            ArgumentException.ThrowIfNullOrWhiteSpace(domain);
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

            var ip = string.Empty;

            try
            {
                await AnsiConsole.Status()
                    .StartAsync("Fetching current IP...", async ctx =>
                    {
                        using var httpClient = new HttpClient();
                        ip = await httpClient.GetStringAsync("https://api.seeip.org", cancellationToken);
                    });
            }
            catch (OperationCanceledException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                return 1;
            }

            var api = new NamecheapApi(username, apiKey, ip, sandbox);

            while (true)
            {
                var hosts = new List<Host>();

                try
                {
                    await AnsiConsole.Status()
                        .StartAsync("Fetching current hosts...", async ctx =>
                        {
                            hosts = await api.GetHostsAsync(domain);
                        });
                }
                catch (OperationCanceledException)
                {
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                    return 1;
                }

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

                AnsiConsole.Write(new Rule(domain));
                AnsiConsole.Write(grid);
                AnsiConsole.WriteLine();

                var selectedOperation = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an operation:")
                    .PageSize(10)
                    .AddChoices([
                        "Refresh hosts",
                        "Add a host",
                        "Update a host",
                        "Delete a host",
                        "Exit"
                    ])
                );

                if (selectedOperation == "Exit")
                {
                    return 0;
                }

                if (selectedOperation == "Refresh hosts")
                {
                    continue;
                }

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

                    hosts.Add(new Host
                    {
                        Hostname = hostname,
                        RecordType = recordType,
                        Address = address
                    });

                    try
                    {
                        await AnsiConsole.Status()
                            .StartAsync("Adding new host...", async ctx =>
                            {
                                await api.SetHostsAsync(domain, hosts);
                            });
                    }
                    catch (OperationCanceledException)
                    {
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                        return 1;
                    }
                }

                if (selectedOperation == "Update a host")
                {
                    var selectedHost = AnsiConsole.Prompt(
                        new SelectionPrompt<Host>()
                            .Title("Select a host to update:")
                            .EnableSearch()
                            .PageSize(3)
                            .MoreChoicesText("Scroll down to see more hosts...")
                            .AddChoices(hosts)
                            .UseConverter(h => $"{h.Hostname} {Enum.GetName(typeof(RecordType), h.RecordType) ?? string.Empty} {h.Address}")
                    );

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

                    try
                    {
                        await AnsiConsole.Status()
                            .StartAsync("Updating host...", async ctx =>
                            {
                                await api.SetHostsAsync(domain, hosts);
                            });
                    }
                    catch (OperationCanceledException)
                    {
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                        return 1;
                    }
                }

                if (selectedOperation == "Delete a host")
                {
                    var selectedHost = AnsiConsole.Prompt(
                        new SelectionPrompt<Host>()
                            .Title("Select a host to delete:")
                            .EnableSearch()
                            .PageSize(3)
                            .MoreChoicesText("Scroll down to see more hosts...")
                            .AddChoices(hosts)
                            .UseConverter(h => $"{h.Hostname} {Enum.GetName(typeof(RecordType), h.RecordType) ?? string.Empty} {h.Address}")
                    );

                    hosts.Remove(selectedHost);

                    try
                    {
                        await AnsiConsole.Status()
                            .StartAsync("Deleting host...", async ctx =>
                            {
                                await api.SetHostsAsync(domain, hosts);
                            });
                    }
                    catch (OperationCanceledException)
                    {
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                        return 1;
                    }
                }
            }
        }
    }
}
