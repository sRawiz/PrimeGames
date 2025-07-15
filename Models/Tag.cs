using System.ComponentModel.DataAnnotations;

namespace PrimeGames.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(20)]
        public string ColorCode { get; set; } = "#6B7280";

        public int UsageCount { get; set; } = 0;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    }
}