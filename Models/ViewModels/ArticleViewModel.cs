using PrimeGames.Models;
using System.ComponentModel.DataAnnotations;

namespace PrimeGames.Models.ViewModels
{
    public class ArticleViewModel
    {
        public Article Article { get; set; } = new();
        public List<Article> RelatedArticles { get; set; } = new();
        public List<Article> PopularArticles { get; set; } = new();
        public bool IsFavorited { get; set; } = false;
        public int FavoriteCount { get; set; } = 0;
    }

    public class ArticleListViewModel
    {
        public List<Article> Articles { get; set; } = new();
        public Category? CurrentCategory { get; set; }
        public Tag? CurrentTag { get; set; }
        public string? SearchTerm { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalArticles { get; set; } = 0;
        public int PageSize { get; set; } = 12;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class CreateArticleViewModel
    {
        [Required]
        [StringLength(500, ErrorMessage = "หัวข้อต้องไม่เกิน 500 ตัวอักษร")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "สรุปต้องไม่เกิน 1000 ตัวอักษร")]
        public string? Summary { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(200, ErrorMessage = "ชื่อผู้เขียนต้องไม่เกิน 200 ตัวอักษร")]
        public string AuthorName { get; set; } = "PrimeGames Team";

        [Required]
        public int CategoryId { get; set; }

        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

        [StringLength(200, ErrorMessage = "Meta Description ต้องไม่เกิน 200 ตัวอักษร")]
        public string? MetaDescription { get; set; }

        [StringLength(500, ErrorMessage = "Meta Keywords ต้องไม่เกิน 500 ตัวอักษร")]
        public string? MetaKeywords { get; set; }

        public IFormFile? FeaturedImage { get; set; }
        public string? FeaturedImageAlt { get; set; }
        public string? ExistingFeaturedImagePath { get; set; }

        public List<IFormFile>? GalleryImages { get; set; }

        public List<int>? SelectedTagIds { get; set; } = new();
        public string? NewTags { get; set; }

        public List<Category> Categories { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();
    }

    public class EditArticleViewModel : CreateArticleViewModel
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<ArticleMedia> ExistingMedia { get; set; } = new();
    }
}