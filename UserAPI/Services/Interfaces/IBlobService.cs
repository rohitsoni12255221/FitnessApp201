namespace UserAPI.AzureServices
{
    public interface IBlobService
    {
        Task<string> UploadAsync(IFormFile file, string fileName);
    }
}
