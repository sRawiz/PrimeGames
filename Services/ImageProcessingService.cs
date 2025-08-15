using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace cleanNETCoreMVC.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly ILogger<ImageProcessingService> _logger;
        private readonly string[] _supportedMimeTypes = {
            "image/jpeg", "image/jpg", "image/png", "image/webp"
        };

        public ImageProcessingService(ILogger<ImageProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> ResizeAndCompressImageAsync(IFormFile imageFile, long maxFileSizeBytes, int maxWidth = 1920, int maxHeight = 1080)
        {
            if (!IsValidImageFile(imageFile))
            {
                throw new ArgumentException("ไฟล์ที่อัปโหลดไม่ใช่รูปภาพที่รองรับ");
            }

            _logger.LogInformation("Starting image processing for file: {FileName}, Original size: {FileSize} bytes", 
                imageFile.FileName, imageFile.Length);

            try
            {
                using var imageStream = imageFile.OpenReadStream();
                using var image = await Image.LoadAsync(imageStream);

                _logger.LogDebug("Original image dimensions: {Width}x{Height}", image.Width, image.Height);

                var (newWidth, newHeight) = CalculateNewDimensions(image.Width, image.Height, maxWidth, maxHeight);

                image.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));

                _logger.LogDebug("Resized image dimensions: {Width}x{Height}", newWidth, newHeight);

                var compressedBytes = await CompressImageToTargetSize(image, maxFileSizeBytes, imageFile.ContentType);

                _logger.LogInformation("Image processing completed. Final size: {FileSize} bytes ({FileSizeMB:F2} MB)", 
                    compressedBytes.Length, compressedBytes.Length / 1024.0 / 1024.0);

                return compressedBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image: {FileName}", imageFile.FileName);
                throw new InvalidOperationException($"เกิดข้อผิดพลาดในการประมวลผลรูปภาพ: {ex.Message}", ex);
            }
        }

        public bool IsValidImageFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return false;

            if (!_supportedMimeTypes.Contains(imageFile.ContentType?.ToLower()))
                return false;

            var extension = Path.GetExtension(imageFile.FileName)?.ToLower();
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            
            return validExtensions.Contains(extension);
        }

        public IFormFile BytesToFormFile(byte[] imageBytes, string fileName, string contentType)
        {
            var stream = new MemoryStream(imageBytes);
            return new FormFile(stream, 0, imageBytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        private static (int width, int height) CalculateNewDimensions(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
        {
            if (originalWidth <= maxWidth && originalHeight <= maxHeight)
                return (originalWidth, originalHeight);

            double ratioX = (double)maxWidth / originalWidth;
            double ratioY = (double)maxHeight / originalHeight;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            return (newWidth, newHeight);
        }

        private async Task<byte[]> CompressImageToTargetSize(Image image, long maxFileSizeBytes, string? contentType)
        {
            int quality = 95;
            byte[] imageBytes;

            do
            {
                using var outputStream = new MemoryStream();
                
                if (contentType?.Contains("png") == true)
                {
                    var pngEncoder = new PngEncoder
                    {
                        CompressionLevel = PngCompressionLevel.BestCompression
                    };
                    await image.SaveAsync(outputStream, pngEncoder);
                }
                else if (contentType?.Contains("webp") == true)
                {
                    var webpEncoder = new WebpEncoder
                    {
                        Quality = quality
                    };
                    await image.SaveAsync(outputStream, webpEncoder);
                }
                else
                {
                    var jpegEncoder = new JpegEncoder
                    {
                        Quality = quality
                    };
                    await image.SaveAsync(outputStream, jpegEncoder);
                }

                imageBytes = outputStream.ToArray();
                
                _logger.LogDebug("Compression attempt with quality {Quality}: {FileSize} bytes", quality, imageBytes.Length);

                if (imageBytes.Length <= maxFileSizeBytes || quality <= 20)
                    break;

                quality -= 10;

            } while (quality > 0);

            if (imageBytes.Length > maxFileSizeBytes)
            {
                _logger.LogWarning("Image still too large after compression. Applying additional resizing.");
                
                int newWidth = (int)(image.Width * 0.8);
                int newHeight = (int)(image.Height * 0.8);
                
                image.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));
                
                using var finalStream = new MemoryStream();
                var finalEncoder = new JpegEncoder { Quality = 60 };
                await image.SaveAsync(finalStream, finalEncoder);
                imageBytes = finalStream.ToArray();
            }

            return imageBytes;
        }
    }
}
