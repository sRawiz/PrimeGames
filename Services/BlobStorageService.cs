using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PrimeGames.Services;

namespace cleanNETCoreMVC.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _blobServiceClient = new BlobServiceClient(
                configuration.GetConnectionString("AzureStorage"));
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty");

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    throw new ArgumentException("Only image files are allowed");

                if (file.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("File size must be less than 5MB");

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(fileName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);

                await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });

                _logger.LogInformation($"File uploaded successfully: {fileName}");
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                throw new Exception($"Error uploading image: {ex.Message}");
            }
        }



        public async Task DeleteImageAsync(string imageUrl, string containerName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                var uri = new Uri(imageUrl);
                var fileName = Path.GetFileName(uri.LocalPath);

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                await blobClient.DeleteIfExistsAsync();
                _logger.LogInformation($"File deleted successfully: {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {imageUrl}");
            }
        }
    }
}