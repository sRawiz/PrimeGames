namespace PrimeGames.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public int UserProfileId { get; set; }
        public int ArticleId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public UserProfile UserProfile { get; set; } = null!;
        public Article Article { get; set; } = null!;
    }
}