using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using WorkoutService.Models;
using WorkoutService.Services.IInterfaces;

namespace WorkoutService.AzureServices
{

    public class AzureQueueConsumer : BackgroundService
    {
        private readonly QueueClient _queueClient;
        private readonly IServiceProvider _serviceProvider;

        public AzureQueueConsumer(IConfiguration config, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _queueClient = new QueueClient(
                config["AzureStorage:ConnectionString"],
                config["AzureStorage:QueueName"]
            );
            _queueClient.CreateIfNotExists();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var response = await _queueClient.ReceiveMessagesAsync(
                        maxMessages: 5,
                        visibilityTimeout: TimeSpan.FromSeconds(30),
                        cancellationToken: stoppingToken
                    );

                    foreach (var message in response.Value)
                    {
                        try
                        {
                            var decoded = JsonSerializer.Deserialize<QueueEventMessage>(message.Body.ToString());

                            if (decoded == null)
                            {
                                throw new InvalidOperationException("Failed to deserialize queue message.");
                            }

                            await HandleEventAsync(decoded);

                            await _queueClient.DeleteMessageAsync(
                                message.MessageId,
                                message.PopReceipt,
                                stoppingToken
                            );
                        }
                        catch (Exception)
                        {
                            // Let your custom exception middleware or global handler deal with this
                            throw;
                        }
                    }
                }
                catch (Exception)
                {
                    // Optional: log or track queue polling-level exceptions
                    // Keep the service alive unless it's critical
                    // You can choose to rethrow here if needed
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task HandleEventAsync(QueueEventMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IWorkout>();

            // Using TryGetProperty to safely access "UserId" in the Payload
            if (message.Payload.TryGetProperty("UserId", out var userIdElement) && userIdElement.ValueKind == JsonValueKind.Number)
            {
                int userId = userIdElement.GetInt32();

                switch (message.ActionType)
                {
                    case "UserDeactivated":
                        await repo.DisableUserWorkoutsAsync(userId);
                        break;

                    case "UserActivated":
                        await repo.EnableUserWorkoutsAsync(userId);
                        break;

                    default:
                        throw new InvalidOperationException($"Unhandled action type: {message.ActionType}");
                }
            }
            else
            {
                // Handle the case when "UserId" is not found or is not a number
                throw new InvalidOperationException("UserId not found or is invalid in the payload.");
            }
        }

    }
}
