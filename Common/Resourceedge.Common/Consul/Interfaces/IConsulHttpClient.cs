using System.Threading.Tasks;

namespace Resourceedge.Common.Consul.Interfaces
{
    public interface IConsulHttpClient
    {
        Task<T> GetAsync<T>(string requestUri);
    }
}

