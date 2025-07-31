using Microsoft.AspNetCore.Mvc;

namespace cleanNETCoreMVC.Areas.Admin
{
    public class AdminAreaAttribute : AreaAttribute
    {
        public AdminAreaAttribute() : base("Admin") { }
    }
} 