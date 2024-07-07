namespace NamecheapAutomation.Services
{
    public interface IIpService
    {
        Task<string> GetIpAsync(CancellationToken cancellationToken);
    }
}