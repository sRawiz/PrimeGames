using PrimeGames.Models;

namespace PrimeGames.Models.ViewModels
{
    public class SearchViewModel
    {
        public string? Query { get; set; }
        public int? CategoryId { get; set; }
        public List<int>? TagIds { get; set; }
        public ArticleStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SortBy { get; set; } = "newest";

        public List<Article> Results { get; set; } = new();
        public int TotalResults { get; set; } = 0;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; } = 1;

        public List<Category> Categories { get; set; } = new();
        public List<Tag> Tags { get; set; } = new();

        public bool HasResults => Results.Any();
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}