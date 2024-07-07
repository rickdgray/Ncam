namespace NamecheapAutomation.Services
{
    public class IpService(HttpClient httpClient) : IIpService
    {
        public async Task<string> GetIpAsync(CancellationToken cancellationToken)
        {
            return await httpClient.GetStringAsync("https://api.seeip.org", cancellationToken);
        }
    }
}
