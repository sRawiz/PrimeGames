using System;
using System.Collections.Generic;

namespace cleanNETCoreMVC.Models
{
    public class Content
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public ApplicationUser Author { get; set; } = default!;
        public ContentCategory Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ViewCount { get; set; } = 0;
        public ICollection<ContentTag> ContentTags { get; set; } = new List<ContentTag>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }

    public enum ContentCategory
    {
        News,
        Article,
        Review,
        Mod,
        Deal,
        Tip
    }
} 