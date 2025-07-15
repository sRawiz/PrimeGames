using System.ComponentModel.DataAnnotations;

namespace PrimeGames.Models
{
    public class ArticleMedia
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }

        [Required]
        [StringLength(500)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(200)]
        public string? AltText { get; set; }

        [StringLength(500)]
        public string? Caption { get; set; }

        public MediaType Type { get; set; } = MediaType.Image;
        public int SortOrder { get; set; } = 0;
        public long FileSize { get; set; } = 0;

        [StringLength(50)]
        public string? MimeType { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string UploadedBy { get; set; } = string.Empty;

        public Article Article { get; set; } = null!;
    }

    public enum MediaType
    {
        Image = 0,
        Video = 1,
        Document = 2
    }
}