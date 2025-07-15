using PrimeGames.Models;

namespace PrimeGames.Models.ViewModels
{
    public class HomeViewModel
    {
        public Article? FeaturedArticle { get; set; }
        public List<Article> LatestNews { get; set; } = new();
        public List<Article> LatestReviews { get; set; } = new();
        public List<Article> HotDeals { get; set; } = new();
        public List<Article> TrendingArticles { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}