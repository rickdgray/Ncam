using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NamecheapAutomation
{
    public class NameCheapApi
    {
        private readonly GlobalParameters _params;
        private readonly XNamespace _ns = XNamespace.Get("http://api.namecheap.com/xml.response");

        public NameCheapApi(string username, string apiUser, string apiKey, string clientIp, bool isSandbox = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(apiUser);
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(clientIp);

            if (IPAddress.TryParse(clientIp, out var ip))
            {
                if (ip.AddressFamily != AddressFamily.InterNetwork)
                {
                    throw new ArgumentException($"Client IP {clientIp} is not a valid IPv4 address.", nameof(clientIp));
                }
            }
            else
            {
                throw new ArgumentException($"{clientIp} does not seem a valid IP address.", nameof(clientIp));
            }

            _params = new GlobalParameters()
            {
                ApiKey = apiKey,
                ApiUser = apiUser,
                CLientIp = clientIp,
                IsSandBox = isSandbox,
                UserName = username
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

            XDocument doc = await query.ExecuteAsync("namecheap.domains.dns.getHosts");

            var serializer = new XmlSerializer(typeof(DnsHostResult), _ns.NamespaceName);

            using var reader = doc.Root.Element(_ns + "CommandResponse").Element(_ns + "DomainDNSGetHostsResult").CreateReader();
            var result = serializer.Deserialize(reader) as DnsHostResult;
            return result ?? throw new Exception("Failed to deserialize response.");
        }
    }
}
