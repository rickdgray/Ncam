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

        public async Task SetHostsAsync(string secondLevelDomain, string topLevelDomain, HostEntry[] hostEntries)
        {
            var query = new Query(_params);
            query.AddParameter("SLD", secondLevelDomain);
            query.AddParameter("TLD", topLevelDomain);

            for (int i = 0; i < hostEntries.Length; i++)
            {
                if ((hostEntries[i].RecordType == RecordType.MX || hostEntries[i].RecordType == RecordType.MXE)
                    && string.IsNullOrEmpty(hostEntries[i].MxPref))
                {
                    throw new ArgumentException("MX record type requires a preference value.", nameof(hostEntries));
                }

                query.AddParameter("HostName" + (i + 1), hostEntries[i].HostName);
                query.AddParameter("Address" + (i + 1), hostEntries[i].Address);
                query.AddParameter("RecordType" + (i + 1), Enum.GetName(typeof(RecordType), hostEntries[i].RecordType) ?? string.Empty);

                if (!string.IsNullOrEmpty(hostEntries[i].MxPref))
                {
                    query.AddParameter("MXPref" + (i + 1), hostEntries[i].MxPref);
                }
                else
                {
                    query.AddParameter("MXPref" + (i + 1), "10");
                }

                if (!string.IsNullOrEmpty(hostEntries[i].Ttl))
                {
                    query.AddParameter("TTL" + (i + 1), hostEntries[i].Ttl);
                }
                else if (hostEntries[i].RecordType == RecordType.ALIAS)
                {
                    query.AddParameter("TTL" + (i + 1), "300");
                }
            }

            await query.ExecuteAsync("namecheap.domains.dns.setHosts");
        }

        public async Task<DnsHostResult> GetHostsAsync(string sld, string tld)
        {
            var query = new Query(_params)
                .AddParameter("SLD", sld)
                .AddParameter("TLD", tld);

            var doc = await query.ExecuteAsync("namecheap.domains.dns.getHosts");

            var serializer = new XmlSerializer(typeof(DnsHostResult), _namespace.NamespaceName);

            using var reader = (doc?.Root
                ?.Element(_namespace + "CommandResponse")
                ?.Element(_namespace + "DomainDNSGetHostsResult")
                ?.CreateReader()) ?? throw new Exception("Received invalid XML response.");

            var result = serializer.Deserialize(reader) as DnsHostResult;
            return result ?? throw new Exception("Failed to deserialize response.");
        }
    }
}
