using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cleanNETCoreMVC.Models;
using System.Threading.Tasks;
using System.Linq;
using PrimeGames.Data;

namespace cleanNETCoreMVC.Controllers
{
    [Authorize]
    [Route("favorites")]
    public class FavoritesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public FavoritesController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var favorites = await _context.Favorites
                .Where(f => f.UserId == user.Id)
                .Include(f => f.Content).ThenInclude(c => c.Author)
                .Include(f => f.Content).ThenInclude(c => c.ContentTags).ThenInclude(ct => ct.Tag)
                .OrderByDescending(f => f.Content.CreatedAt)
                .ToListAsync();
            return View(favorites);
        }

        [HttpPost("add/{contentId}")]
        public async Task<IActionResult> Add(int contentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            if (!_context.Favorites.Any(f => f.UserId == user.Id && f.ContentId == contentId))
            {
                _context.Favorites.Add(new Favorite { UserId = user.Id, ContentId = contentId, CreatedAt = System.DateTime.Now });
                await _context.SaveChangesAsync();
                TempData["FavoriteSuccess"] = "เพิ่มข่าวนี้ในรายการโปรดเรียบร้อยแล้ว";
            }
            else
            {
                TempData["FavoriteSuccess"] = "ข่าวนี้อยู่ในรายการโปรดแล้ว";
            }
            var referer = Request.Headers["Referer"].FirstOrDefault();
            return Redirect(referer ?? "/");
        }

        [HttpPost("remove/{contentId}")]
        public async Task<IActionResult> Remove(int contentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var fav = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == user.Id && f.ContentId == contentId);
            if (fav != null)
            {
                _context.Favorites.Remove(fav);
                await _context.SaveChangesAsync();
                TempData["FavoriteSuccess"] = "ลบข่าวนี้ออกจากรายการโปรดแล้ว";
            }
            else
            {
                TempData["FavoriteSuccess"] = "ข่าวนี้ไม่ได้อยู่ในรายการโปรด";
            }
            var referer = Request.Headers["Referer"].FirstOrDefault();
            return Redirect(referer ?? "/");
        }
    }
} 