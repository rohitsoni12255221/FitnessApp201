namespace UserAPI.AzureServices
{
    public interface IQueueService
    {
        Task SendMessageAsync(string actionType, object payload, string source = null);
    }
}
