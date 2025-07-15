namespace PrimeGames.Models.DTOs
{
    public class ArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? FeaturedImagePath { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int ViewCount { get; set; }
        public int FavoriteCount { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorCode { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
    }

    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }
}