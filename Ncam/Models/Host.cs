namespace Ncam.Models
{
    public class Host
    {
        public int Id { get; set; }
        public string Hostname { get; set; } = string.Empty;
        public RecordType RecordType { get; set; }
        public string Address { get; set; } = string.Empty;
        public string MxPref { get; set; } = "10";
        public string Ttl { get; set; } = string.Empty;
    }
}
