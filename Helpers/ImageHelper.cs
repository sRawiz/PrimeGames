namespace PrimeGames.Helpers
{
    public static class ImageHelper
    {
        public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public static readonly string[] AllowedMimeTypes = {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"
        };

        public static readonly long MaxFileSize = 5 * 1024 * 1024;

        public static bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return false;

            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        public static string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileName = Path.GetFileNameWithoutExtension(originalFileName);
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            return $"{fileName}_{uniqueId}{extension}";
        }

        public static string GetImageUrl(string? imagePath, string defaultImage = "/images/default-article.jpg")
        {
            if (string.IsNullOrEmpty(imagePath))
                return defaultImage;

            if (imagePath.StartsWith("http"))
                return imagePath;

            return imagePath.StartsWith("/") ? imagePath : $"/{imagePath}";
        }
    }
}