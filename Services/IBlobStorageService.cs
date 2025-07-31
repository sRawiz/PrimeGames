namespace PrimeGames.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string containerName);
        Task DeleteImageAsync(string imageUrl, string containerName);
    }
}
