using Microsoft.Extensions.Options;
using Ncam.Models;
using Spectre.Console;

namespace Ncam.Services
{
    public class NamecheapService(IOptions<GlobalParameters> parameters,
        IAnsiConsole console,
        HttpClient httpClient) : INamecheapService
    {
        private readonly GlobalParameters _parameters = parameters.Value;
        private readonly HttpClient _httpClient = httpClient;

        public async Task SetHostsAsync(IEnumerable<Host> hosts)
        {
            try
            {
                await console.Status()
                    .StartAsync("Adding new host...", async ctx =>
                    {
                        var (sld, tld) = _parameters.Domain.Split('.') switch { var a => (a[0], a[1]) };
                        var query = new Query(_parameters, _httpClient)
                            .SetParameter("SLD", sld)
                            .SetParameter("TLD", tld);

                        // TODO: use hosts.Index() when .net 9 is stable
                        foreach (var (i, host) in hosts.Select((x, i) => (i, x)))
                        {
                            if ((host.RecordType == RecordType.MX || host.RecordType == RecordType.MXE)
                                && string.IsNullOrEmpty(host.MxPref))
                            {
                                throw new ArgumentException("MX record type requires a preference value.", nameof(hosts));
                            }

                            query.SetParameter($"HostName{i + 1}", host.Hostname);
                            query.SetParameter($"Address{i + 1}", host.Address);
                            query.SetParameter($"RecordType{i + 1}", Enum.GetName(typeof(RecordType), host.RecordType) ?? string.Empty);

                            if (!string.IsNullOrEmpty(host.MxPref))
                            {
                                query.SetParameter($"MXPref{i + 1}", host.MxPref);
                            }
                            else
                            {
                                query.SetParameter($"MXPref{i + 1}", "10");
                            }

                            if (!string.IsNullOrEmpty(host.Ttl))
                            {
                                query.SetParameter($"TTL{i + 1}", host.Ttl);
                            }

                            // alias require TTL to be 300 so we force it here
                            if (host.RecordType == RecordType.ALIAS)
                            {
                                query.SetParameter($"TTL{i + 1}", "300");
                            }
                        }

                        await query.ExecuteAsync("namecheap.domains.dns.setHosts");
                    });
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        public async Task<List<Host>> GetHostsAsync()
        {
            var hosts = new List<Host>();

            try
            {
                await console.Status()
                    .StartAsync("Fetching current hosts...", async ctx =>
                    {
                        var (sld, tld) = _parameters.Domain.Split('.') switch { var a => (a[0], a[1]) };
                        var query = new Query(_parameters, _httpClient)
                            .SetParameter("SLD", sld)
                            .SetParameter("TLD", tld);

                        var xml = await query.ExecuteAsync("namecheap.domains.dns.getHosts");

                        hosts = XmlHelper.ParseHostResponse(xml);
                    });
            }
            catch (OperationCanceledException)
            {
                return [];
            }

            return hosts;
        }
    }
}
