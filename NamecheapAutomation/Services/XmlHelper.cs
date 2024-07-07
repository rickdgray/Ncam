using NamecheapAutomation.Models;
using System.Xml.Linq;

namespace NamecheapAutomation.Services
{
    public static class XmlHelper
    {
        public readonly static XNamespace Namespace = "http://api.namecheap.com/xml.response";
        public static List<Host> ParseHostResponse(XDocument doc)
        {
            var root = doc.Root ?? throw new Exception("No root element found in the response.");

            var status = root.Attribute("Status")?.Value ?? string.Empty;

            if (status.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                var errors = root.Descendants("Error")
                    .Select(x => x.Value)
                    .ToArray() ?? [];

                throw new Exception(string.Join(",", errors));
            }

            var hostElements = root
                .Element(Namespace + "CommandResponse")
                ?.Element(Namespace + "DomainDNSGetHostsResult")
                ?.Elements(Namespace + "host") ?? [];

            var hosts = new List<Host>();

            foreach (var hostElement in hostElements)
            {
                hosts.Add(new Host
                {
                    Id = int.TryParse(hostElement.Attribute("HostId")?.Value, out var id) ? id : -1,
                    Hostname = hostElement.Attribute("Name")?.Value ?? string.Empty,
                    Address = hostElement.Attribute("Address")?.Value ?? string.Empty,
                    RecordType = Enum.Parse<RecordType>(hostElement.Attribute("Type")?.Value ?? "UKNOWN"),
                    MxPref = hostElement.Attribute("MXPref")?.Value ?? string.Empty,
                    Ttl = hostElement.Attribute("TTL")?.Value ?? string.Empty
                });
            }

            return hosts;
        }
    }
}
