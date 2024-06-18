namespace NamecheapAutomation
{
    public class NamecheapApi(GlobalParameters parameters)
    {
        private readonly GlobalParameters _params = parameters;

        public async Task SetHostsAsync(IEnumerable<Host> hosts)
        {
            var (sld, tld) = _params.Domain.Split('.') switch { var a => (a[0], a[1]) };
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

        public async Task<List<Host>> GetHostsAsync()
        {
            var (sld, tld) = _params.Domain.Split('.') switch { var a => (a[0], a[1]) };
            var query = new Query(_params)
                .SetParameter("SLD", sld)
                .SetParameter("TLD", tld);

            var xml = await query.ExecuteAsync("namecheap.domains.dns.getHosts");

            return XmlHelper.ParseHostResponse(xml);
        }
    }
}
