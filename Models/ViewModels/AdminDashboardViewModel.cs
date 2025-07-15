namespace PrimeGames.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalArticles { get; set; }
        public int PublishedArticles { get; set; }
        public int DraftArticles { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public int TotalTags { get; set; }

        public List<Article> RecentArticles { get; set; } = new();
        public List<Article> PopularArticles { get; set; } = new();
        public List<UserProfile> RecentUsers { get; set; } = new();

        public Dictionary<string, int> ArticlesByCategory { get; set; } = new();
        public Dictionary<string, int> ArticlesByMonth { get; set; } = new();
        public int TodayViews { get; set; }
        public int ThisMonthViews { get; set; }
    }
}