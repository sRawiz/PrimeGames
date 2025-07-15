namespace PrimeGames.Models.ViewModels
{
    public class FavoriteViewModel
    {
        public List<Article> FavoriteArticles { get; set; } = new();
        public int TotalFavorites { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}