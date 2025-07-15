using System.ComponentModel.DataAnnotations;

namespace PrimeGames.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(20)]
        public string ColorCode { get; set; } = "#3B82F6";

        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}