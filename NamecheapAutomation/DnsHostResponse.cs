using System.Xml.Serialization;

namespace NamecheapAutomation
{
    public class DnsHostResponse
    {
        public List<Host> Hosts { get; set; } = [];
    }
}
