using System.Xml.Serialization;

namespace NamecheapAutomation
{
    public class HostEntry
    {
        [XmlAttribute("HostId")]
        public int Id { get; set; }
        [XmlAttribute("Name")]
        public string HostName { get; set; } = string.Empty;
        [XmlAttribute("Type")]
        public RecordType RecordType { get; set; }
        [XmlAttribute("Address")]
        public string Address { get; set; } = string.Empty;
        [XmlAttribute("MXPref")]
        public string MxPref { get; set; } = "10";
        [XmlAttribute("TTL")]
        public string Ttl { get; set; } = string.Empty;
    }
}
