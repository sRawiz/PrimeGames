using System.ComponentModel.DataAnnotations;

namespace PrimeGames.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Summary { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string AuthorName { get; set; } = string.Empty;

        [StringLength(450)]
        public string? AuthorId { get; set; }

        [StringLength(500)]
        public string? FeaturedImagePath { get; set; }

        [StringLength(200)]
        public string? FeaturedImageAlt { get; set; }

        public int CategoryId { get; set; }
        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedDate { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        public int ViewCount { get; set; } = 0;
        public int FavoriteCount { get; set; } = 0;

        [StringLength(200)]
        public string? MetaDescription { get; set; }

        [StringLength(500)]
        public string? MetaKeywords { get; set; }

        public Category Category { get; set; } = null!;
        public ICollection<ArticleMedia> Media { get; set; } = new List<ArticleMedia>();
        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }

    public enum ArticleStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }
}