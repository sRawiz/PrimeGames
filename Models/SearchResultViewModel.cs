using System.Collections.Generic;
using cleanNETCoreMVC.Models;

namespace cleanNETCoreMVC.Models
{
    public class SearchResultViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<Content> Results { get; set; } = new List<Content>();
        public List<Content> SuggestNews { get; set; } = new List<Content>();
    }
} 