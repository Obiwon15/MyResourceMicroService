using System.Threading.Tasks;
using Consul;

namespace Resourceedge.Common.Consul.Interfaces
{
    public interface IConsulServicesRegistry
    {
        Task<AgentService> GetAsync(string name);
    }
}