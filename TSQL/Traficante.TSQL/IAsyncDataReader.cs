using System.Data;
using System.Threading.Tasks;

namespace Traficante.TSQL
{
    public interface IAsyncDataReader : IDataReader
    {
        Task<bool> NextResultAsync();
        Task<bool> ReadAsync();
    }

}
