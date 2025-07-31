using System;

namespace cleanNETCoreMVC.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = default!;
        public int ContentId { get; set; }
        public Content Content { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
} 