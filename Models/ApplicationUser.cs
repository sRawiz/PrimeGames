using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace cleanNETCoreMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Content> Contents { get; set; } = new List<Content>();
    }
} 