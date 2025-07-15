using System.ComponentModel.DataAnnotations;

namespace PrimeGames.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "ชื่อต้องไม่เกิน 100 ตัวอักษร")]
        public string? FirstName { get; set; }

        [StringLength(100, ErrorMessage = "นามสกุลต้องไม่เกิน 100 ตัวอักษร")]
        public string? LastName { get; set; }

        public Gender? Gender { get; set; }

        [Display(Name = "วันเกิด")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20, ErrorMessage = "เบอร์โทรต้องไม่เกิน 20 ตัวอักษร")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรไม่ถูกต้อง")]
        public string? PhoneNumber { get; set; }

        [StringLength(500, ErrorMessage = "การแนะนำตัวต้องไม่เกิน 500 ตัวอักษร")]
        public string? Bio { get; set; }

        public string? AvatarPath { get; set; }
        public IFormFile? AvatarFile { get; set; }

        public bool EmailNotifications { get; set; } = true;
        public bool NewsletterSubscription { get; set; } = true;

        public int FavoriteCount { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime JoinDate { get; set; }
    }
}