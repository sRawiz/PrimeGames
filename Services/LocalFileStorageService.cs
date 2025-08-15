using PrimeGames.Services;

namespace cleanNETCoreMVC.Services
{
    public class LocalFileStorageService : IBlobStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _uploadsPath;

        public LocalFileStorageService(IWebHostEnvironment environment, ILogger<LocalFileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
            _uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            
            // Create uploads directory if it doesn't exist
            if (!Directory.Exists(_uploadsPath))
            {
                Directory.CreateDirectory(_uploadsPath);
                _logger.LogInformation("Created uploads directory: {Path}", _uploadsPath);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty");

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    throw new ArgumentException("Only image files are allowed");

                if (file.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("File size must be less than 5MB");

                // Generate unique filename with prefix
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"thumb_{Guid.NewGuid():N}{fileExtension}";
                var filePath = Path.Combine(_uploadsPath, fileName);

                _logger.LogInformation("Uploading file locally: {FileName}, Size: {Size} bytes", fileName, file.Length);

                // Save file to wwwroot/uploads
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative URL path
                var relativeUrl = $"/uploads/{fileName}";
                
                _logger.LogInformation("File uploaded successfully to local storage: {FileName} -> {RelativeUrl}", fileName, relativeUrl);
                return relativeUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to local storage");
                throw new Exception($"Error uploading image: {ex.Message}");
            }
        }



        public async Task DeleteImageAsync(string imageUrl, string containerName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                // Handle both absolute and relative URLs
                string fileName;
                if (imageUrl.StartsWith("/uploads/"))
                {
                    fileName = Path.GetFileName(imageUrl);
                }
                else if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                {
                    fileName = Path.GetFileName(uri.LocalPath);
                }
                else
                {
                    fileName = Path.GetFileName(imageUrl);
                }

                var filePath = Path.Combine(_uploadsPath, fileName);

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation("File deleted successfully from local storage: {FileName}", fileName);
                }
                else
                {
                    _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from local storage: {ImageUrl}", imageUrl);
            }
        }
    }
}
