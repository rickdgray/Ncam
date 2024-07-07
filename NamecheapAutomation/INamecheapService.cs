
namespace NamecheapAutomation
{
    public interface INamecheapService
    {
        Task<List<Host>> GetHostsAsync();
        Task SetHostsAsync(IEnumerable<Host> hosts);
    }
}