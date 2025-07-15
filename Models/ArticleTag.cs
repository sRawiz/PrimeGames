namespace PrimeGames.Models
{
    public class ArticleTag
    {
        public int ArticleId { get; set; }
        public int TagId { get; set; }

        public Article Article { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}