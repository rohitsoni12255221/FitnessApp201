using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;

namespace UserAPI.AzureServices
{
    public class AzureQueueService : IQueueService
    {
        private readonly QueueClient _queueClient;

        public AzureQueueService(IConfiguration configuration)
        {
            string connectionString = configuration["AzureStorage:ConnectionString"];
            string queueName = configuration["AzureStorage:QueueName"];

            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync(string actionType, object payload, string source = null)
        {
            var message = new
            {
                ActionType = actionType,
                Payload = payload,
                Source = source ?? "System",
                TimeStamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            await _queueClient.SendMessageAsync(json);
        }
    }
}
