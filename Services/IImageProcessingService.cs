namespace cleanNETCoreMVC.Services
{
    public interface IImageProcessingService
    {
        Task<byte[]> ResizeAndCompressImageAsync(IFormFile imageFile, long maxFileSizeBytes, int maxWidth = 1920, int maxHeight = 1080);

        bool IsValidImageFile(IFormFile imageFile);

        IFormFile BytesToFormFile(byte[] imageBytes, string fileName, string contentType);
    }
}
