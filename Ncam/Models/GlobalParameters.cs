namespace Ncam.Models
{
    public class GlobalParameters
    {
        public string Domain { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ClientIp { get; set; } = string.Empty;
        public bool IsSandBox { get; set; }
    }
}
