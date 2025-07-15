namespace PrimeGames.Constants
{
    public static class AppConstants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string User = "User";
        }

        public static class ImagePaths
        {
            public const string ArticleImages = "images/articles";
            public const string FeaturedImages = "images/articles/featured";
            public const string GalleryImages = "images/articles/gallery";
            public const string ThumbnailImages = "images/articles/thumbnails";
            public const string UserAvatars = "images/avatars";
            public const string TempUploads = "images/uploads/temp";
        }

        public static class CacheKeys
        {
            public const string Categories = "categories";
            public const string PopularTags = "popular_tags";
            public const string LatestArticles = "latest_articles";
            public const string PopularArticles = "popular_articles";
        }

        public static class Pagination
        {
            public const int DefaultPageSize = 12;
            public const int AdminPageSize = 20;
            public const int MaxPageSize = 50;
        }

        public static class ArticleSettings
        {
            public const int SummaryMaxLength = 200;
            public const int RelatedArticlesCount = 4;
            public const int PopularArticlesCount = 5;
            public const int LatestNewsCount = 6;
        }
    }
}