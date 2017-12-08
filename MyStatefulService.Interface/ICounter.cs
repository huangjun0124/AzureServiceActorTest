using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace MyStatefulService.Interface
{
    public interface ICounter : IService
    {
        Task<long> GetCountAsync();

        Task<CounterClass> GetCounterClassAsync();
    }
}
