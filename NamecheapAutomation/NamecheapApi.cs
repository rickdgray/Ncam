using System.Net.Sockets;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NamecheapAutomation
{
    public class NamecheapApi
    {
        private readonly XNamespace _namespace = "http://api.namecheap.com/xml.response";
        private readonly GlobalParameters _params;

        public NamecheapApi(string username, string apiKey, string clientIp, bool isSandbox = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

            if (!IPAddress.TryParse(clientIp, out var ip))
            {
                throw new ArgumentException($"{clientIp} does not seem to be a valid IP address.", nameof(clientIp));
            }

            if (ip.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException($"Client IP {clientIp} is not a valid IPv4 address.", nameof(clientIp));
            }

            _params = new GlobalParameters()
            {
                UserName = username,
                ApiKey = apiKey,
                ClientIp = clientIp,
                IsSandBox = isSandbox
            };
        }

        public async Task SetHostsAsync(string domain, IEnumerable<Host> hosts)
        {
            var (sld, tld) = domain.Split('.') switch { var a => (a[0], a[1]) };
            var query = new Query(_params)
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
        }

        public async Task<List<Host>> GetHostsAsync(string domain)
        {
            var (sld, tld) = domain.Split('.') switch { var a => (a[0], a[1]) };
            var query = new Query(_params)
                .SetParameter("SLD", sld)
                .SetParameter("TLD", tld);

            var doc = await query.ExecuteAsync("namecheap.domains.dns.getHosts");

            var serializer = new XmlSerializer(typeof(DnsHostResponse), _namespace.NamespaceName);

            using var reader = (doc?.Root
                ?.Element(_namespace + "CommandResponse")
                ?.Element(_namespace + "DomainDNSGetHostsResult")
                ?.CreateReader()) ?? throw new Exception("Received invalid XML response.");

            var result = serializer.Deserialize(reader) as DnsHostResponse;
            return result?.Hosts ?? throw new Exception("Failed to deserialize response.");
        }
    }
}
