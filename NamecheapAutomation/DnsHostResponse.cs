using System.Xml.Serialization;

namespace NamecheapAutomation
{
    [XmlRoot("DomainDNSGetHostsResult")]
    public class DnsHostResponse
    {
        [XmlElement("host")]
        public List<Host> Hosts { get; set; } = [];
    }
}
