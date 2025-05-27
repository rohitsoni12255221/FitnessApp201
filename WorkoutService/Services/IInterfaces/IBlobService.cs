namespace WorkoutService.Services.Implementaion
{

    public interface IBlobService
    {
        Task<string> UploadAsync(IFormFile file, string fileName);
    }
}
