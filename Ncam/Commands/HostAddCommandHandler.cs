using Microsoft.Extensions.Options;
using Ncam.Models;
using Ncam.Services;
using Spectre.Console;
using System.CommandLine.Invocation;

namespace Ncam.Commands
{
    public class HostAddCommandHandler(IOptions<GlobalParameters> parameters,
        IAnsiConsole console,
        IIpService ipService,
        INamecheapService namecheapService) : ICommandHandler
    {
        private readonly GlobalParameters _parameters = parameters.Value;
        private readonly IIpService _ipService = ipService;
        private readonly INamecheapService _namecheapService = namecheapService;

        public int Invoke(InvocationContext context)
        {
            throw new NotImplementedException();
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var cancellationToken = context.GetCancellationToken();

            var domain = context.ParseResult.GetValueForOption(GlobalOptions.Domain);
            var username = context.ParseResult.GetValueForOption(GlobalOptions.Username);
            var apiKey = context.ParseResult.GetValueForOption(GlobalOptions.ApiKey);
            var sandbox = context.ParseResult.GetValueForOption(GlobalOptions.Sandbox);

            var hosts = context.ParseResult.GetValueForOption(HostOptions.Hostname);
            var recordTypes = context.ParseResult.GetValueForOption(HostOptions.RecordType);
            var addresses = context.ParseResult.GetValueForOption(HostOptions.Address);

            ArgumentException.ThrowIfNullOrWhiteSpace(domain);
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

            // TODO: better argument validation
            if (hosts?.Count == 0)
            {
                console.MarkupLine("[red]No hosts specified.[/]");
                return 1;
            }

            if (hosts.Count != recordTypes.Count || hosts.Count != addresses.Count)
            {
                console.MarkupLine("[red]Hosts, record types, and addresses must have the same number of values.[/]");
                return 1;
            }

            _parameters.Domain = domain;
            _parameters.UserName = username;
            _parameters.ApiKey = apiKey;
            _parameters.IsSandBox = sandbox;

            _parameters.ClientIp = await _ipService.GetIpAsync(cancellationToken);

            var currentHosts = await _namecheapService.GetHostsAsync();

            foreach (var (host, recordType, address) in hosts.Zip(recordTypes, addresses))
            {
                currentHosts.Add(new Host
                {
                    Hostname = host,
                    RecordType = Enum.Parse<RecordType>(recordType, true),
                    Address = string.IsNullOrWhiteSpace(address) ? _parameters.ClientIp : address
                });
            }

            await _namecheapService.SetHostsAsync(currentHosts);

            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();
            grid.AddColumn();

            grid.AddRow([
                new Text("Host Name", new Style(Color.Green, Color.Black)),
                new Text("Record Type", new Style(Color.Red, Color.Black)).RightJustified(),
                new Text("Address", new Style(Color.Blue, Color.Black))
            ]);

            foreach (var host in await _namecheapService.GetHostsAsync())
            {
                grid.AddRow([
                    new Text(host.Hostname),
                    new Text(Enum.GetName(typeof(RecordType), host.RecordType) ?? string.Empty).RightJustified(),
                    new Text(host.Address)
                ]);
            }

            console.Write(new Rule(_parameters.Domain));
            console.Write(grid);

            return 0;
        }
    }
}
