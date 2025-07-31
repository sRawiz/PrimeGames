using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrimeGames.Data;
using cleanNETCoreMVC.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace cleanNETCoreMVC.Controllers
{
    [Route("")]
    public class ContentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ContentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("{category:alpha}/{slug}")]
        public async Task<IActionResult> Detail(string category, string slug)
        {
            if (!System.Enum.TryParse<ContentCategory>(category, true, out var cat))
                return NotFound();
            var content = await _context.Contents
                .Include(c => c.Author)
                .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync(c => c.Category == cat && c.Slug == slug);
            if (content == null) return NotFound();

            // เพิ่มจำนวนการเข้าชม
            content.ViewCount++;
            await _context.SaveChangesAsync();

            bool isFavorite = false;
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                isFavorite = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ContentId == content.Id);
            }
            ViewBag.IsFavorite = isFavorite;

            // ดึงข่าวอื่นๆ (ยกเว้นข่าวปัจจุบัน)
            var otherNews = await _context.Contents
                .Where(c => c.Id != content.Id && c.Category == content.Category)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.OtherNews = otherNews;

            return View("Detail", content);
        }

        [HttpGet("category/{category:alpha}")]
        public async Task<IActionResult> ByCategory(string category)
        {
            if (!System.Enum.TryParse<ContentCategory>(category, true, out var cat))
                return NotFound();
            var contents = await _context.Contents
                .Include(c => c.Author)
                .Where(c => c.Category == cat)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View("ByCategory", contents);
        }

        [HttpGet("tag/{slug}")]
        public async Task<IActionResult> ByTag(string slug)
        {
            var tag = await _context.Tags
                .Include(t => t.ContentTags).ThenInclude(ct => ct.Content).ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(t => t.Slug == slug);
            if (tag == null) return NotFound();
            var contents = tag.ContentTags.Select(ct => ct.Content)
                .OrderByDescending(c => c.CreatedAt).ToList();
            ViewBag.TagName = tag.Name;
            return View("ByTag", contents);
        }

        [HttpGet("news")]
        public async Task<IActionResult> News()
        {
            return await ByCategory("news");
        }

        [HttpGet("article")]
        public async Task<IActionResult> Articles()
        {
            return await ByCategory("article");
        }

        [HttpGet("review")]
        public async Task<IActionResult> Reviews()
        {
            return await ByCategory("review");
        }

        [HttpGet("mod")]
        public async Task<IActionResult> Mods()
        {
            return await ByCategory("mod");
        }

        [HttpGet("deal")]
        public async Task<IActionResult> Deals()
        {
            return await ByCategory("deal");
        }

        [HttpGet("tip")]
        public async Task<IActionResult> Tips()
        {
            return await ByCategory("tip");
        }
    }
} 