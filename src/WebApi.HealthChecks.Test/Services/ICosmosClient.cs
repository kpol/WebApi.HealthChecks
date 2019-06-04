using System.Threading.Tasks;

namespace WebApi.HealthChecks.Test.Services
{
    public interface ICosmosClient
    {
        Task<bool> Connect();
    }

    public class CosmosClient : ICosmosClient
    {
        public Task<bool> Connect()
        {
            return Task.FromResult(false);
        }
    }
}