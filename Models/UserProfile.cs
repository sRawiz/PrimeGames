using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PrimeGames.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(500)]
        public string? AvatarPath { get; set; }

        public bool EmailNotifications { get; set; } = true;
        public bool NewsletterSubscription { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public IdentityUser User { get; set; } = null!;
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }

    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2
    }
}