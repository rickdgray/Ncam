using NamecheapAutomation.Models;

namespace NamecheapAutomation.Services
{
    public interface INamecheapService
    {
        Task<List<Host>> GetHostsAsync();
        Task SetHostsAsync(IEnumerable<Host> hosts);
    }
}