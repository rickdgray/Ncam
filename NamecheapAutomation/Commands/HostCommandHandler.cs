using Microsoft.Extensions.Options;
using NamecheapAutomation.Services;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NamecheapAutomation.Commands
{
    public class HostCommandHandler(IOptions<GlobalParameters> parameters,
        IAnsiConsole console,
        IIpService ipService,
        INamecheapService namecheapService) : ICommandHandler
    {
        public static readonly Option<List<string>> Hostname = new(["--hostname", "-h"], "The host name.");
        public static readonly Option<List<string>> RecordType = new(["--recordType", "-r"], "The record type.");
        public static readonly Option<List<string>> Address = new(["--address", "-a"], "The address.");

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

            var ip = await _ipService.GetIpAsync(cancellationToken);

            ArgumentException.ThrowIfNullOrWhiteSpace(domain);
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(ip);

            _parameters.Domain = domain;
            _parameters.UserName = username;
            _parameters.ApiKey = apiKey;
            _parameters.ClientIp = ip;
            _parameters.IsSandBox = sandbox;

            var hosts = await _namecheapService.GetHostsAsync();

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

            console.Write(new Rule(_parameters.Domain));
            console.Write(grid);

            return 0;
        }
    }
}
