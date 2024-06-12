using System.Xml.Serialization;

namespace NamecheapAutomation
{
    [XmlRoot("DomainDNSGetHostsResult")]
    public class DnsHostResult
    {
        [XmlElement("host")]
        public List<Host> Hosts { get; set; } = [];
    }
}
