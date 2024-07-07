using Ncam.Models;

namespace Ncam.Services
{
    public interface INamecheapService
    {
        Task<List<Host>> GetHostsAsync();
        Task SetHostsAsync(IEnumerable<Host> hosts);
    }
}