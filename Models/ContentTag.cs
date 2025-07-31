namespace cleanNETCoreMVC.Models
{
    public class ContentTag
    {
        public int ContentId { get; set; }
        public Content Content { get; set; } = default!;
        public int TagId { get; set; }
        public Tag Tag { get; set; } = default!;
    }
} 