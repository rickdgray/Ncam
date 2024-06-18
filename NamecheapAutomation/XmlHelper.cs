using System.Xml.Linq;

namespace NamecheapAutomation
{
    public static class XmlHelper
    {
        public static List<Host> ParseHostResponse(XDocument doc)
        {
            XNamespace ns = "http://api.namecheap.com/xml.response";

            var root = doc.Root;

            string status = root.Attribute("Status")?.Value;
            Console.WriteLine($"ApiResponse Status: {status}");

            if (root?.Attribute("Status")?.Value.Equals("ERROR", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                throw new Exception(string.Join(",",
                    root?.Descendants("Error")
                        .Select(x => x.Value)
                        .ToArray()));
            }

            string requestedCommand = root.Element(ns + "RequestedCommand")?.Value;

            var commandResponse = root.Element(ns + "CommandResponse");

            var domainDNSGetHostsResult = commandResponse.Element(ns + "DomainDNSGetHostsResult");

            var hosts = new List<Host>();

            foreach (var hostElement in domainDNSGetHostsResult.Elements(ns + "host"))
            {
                var host = new Host
                {
                    Id = int.TryParse(hostElement.Attribute("HostId")?.Value, out var id) ? id : -1,
                    Hostname = hostElement.Attribute("Name")?.Value,
                    Address = hostElement.Attribute("Address")?.Value,
                    RecordType = Enum.Parse<RecordType>(hostElement.Attribute("Type")?.Value),
                    MxPref = hostElement.Attribute("MXPref")?.Value,
                    Ttl = hostElement.Attribute("TTL")?.Value
                };

                hosts.Add(host);
            }

            return hosts;
        }
    }
}
