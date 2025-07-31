using System.Collections.Generic;

namespace cleanNETCoreMVC.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public ICollection<ContentTag> ContentTags { get; set; } = new List<ContentTag>();
    }
} 