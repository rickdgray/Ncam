using Spectre.Console;

namespace Ncam.Services
{
    public class IpService(HttpClient httpClient,
        IAnsiConsole console) : IIpService
    {
        public async Task<string> GetIpAsync(CancellationToken cancellationToken)
        {
            var ip = string.Empty;

            try
            {
                await console
                    .Status()
                    .StartAsync("Fetching current IP...", async ctx =>
                    {
                        ip = await httpClient.GetStringAsync("https://api.seeip.org", cancellationToken);
                    });
            }
            catch (OperationCanceledException)
            {
                return string.Empty;
            }

            return ip;
        }
    }
}
