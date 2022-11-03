using System.Threading.Tasks;

namespace Resourceedge.Common.Fabio.Interfaces
{
    public interface IFabioHttpClient
    {
        Task<T> GetAsync<T>(string requestUri);
    }
}