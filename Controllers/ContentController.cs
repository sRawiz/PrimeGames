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

            var contentQuery = from c in _context.Contents
                              join a in _context.Users on c.AuthorId equals a.Id
                              where c.Category == cat && c.Slug == slug
                              select new Content
                              {
                                  Id = c.Id,
                                  Title = c.Title,
                                  Slug = c.Slug,
                                  Body = c.Body,
                                  ThumbnailUrl = c.ThumbnailUrl,
                                  AuthorId = c.AuthorId,
                                  Category = c.Category,
                                  CreatedAt = c.CreatedAt,
                                  UpdatedAt = c.UpdatedAt,
                                  ViewCount = c.ViewCount,
                                  Author = new ApplicationUser
                                  {
                                      Id = a.Id,
                                      FirstName = a.FirstName,
                                      LastName = a.LastName,
                                      UserName = a.UserName
                                  }
                              };

            var content = await contentQuery.FirstOrDefaultAsync();
            if (content == null) return NotFound();

            var contentTags = await (from ct in _context.ContentTags
                                   join t in _context.Tags on ct.TagId equals t.Id
                                   where ct.ContentId == content.Id
                                   select new ContentTag
                                   {
                                       ContentId = ct.ContentId,
                                       TagId = ct.TagId,
                                       Tag = new Tag
                                       {
                                           Id = t.Id,
                                           Name = t.Name,
                                           Slug = t.Slug
                                       }
                                   }).ToListAsync();
            
            content.ContentTags = contentTags;

            var contentToUpdate = await _context.Contents.FindAsync(content.Id);
            if (contentToUpdate != null)
            {
                contentToUpdate.ViewCount++;
                await _context.SaveChangesAsync();
                content.ViewCount = contentToUpdate.ViewCount;
            }

            bool isFavorite = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    isFavorite = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ContentId == content.Id);
                }
            }
            ViewBag.IsFavorite = isFavorite;

            var otherNews = await (from c in _context.Contents
                                 where c.Id != content.Id && c.Category == content.Category
                                 orderby c.CreatedAt descending
                                 select new Content
                                 {
                                     Id = c.Id,
                                     Title = c.Title,
                                     Slug = c.Slug,
                                     ThumbnailUrl = c.ThumbnailUrl,
                                     Category = c.Category,
                                     CreatedAt = c.CreatedAt
                                 }).Take(5).ToListAsync();
            ViewBag.OtherNews = otherNews;

            return View("Detail", content);
        }

        [HttpGet("category/{category:alpha}")]
        public async Task<IActionResult> ByCategory(string category)
        {
            if (!System.Enum.TryParse<ContentCategory>(category, true, out var cat))
                return NotFound();

            var contents = await (from c in _context.Contents
                                join a in _context.Users on c.AuthorId equals a.Id
                                where c.Category == cat
                                orderby c.CreatedAt descending
                                select new Content
                                {
                                    Id = c.Id,
                                    Title = c.Title,
                                    Slug = c.Slug,
                                    Body = c.Body,
                                    ThumbnailUrl = c.ThumbnailUrl,
                                    AuthorId = c.AuthorId,
                                    Category = c.Category,
                                    CreatedAt = c.CreatedAt,
                                    ViewCount = c.ViewCount,
                                    Author = new ApplicationUser
                                    {
                                        Id = a.Id,
                                        FirstName = a.FirstName,
                                        LastName = a.LastName,
                                        UserName = a.UserName
                                    }
                                }).ToListAsync();
                                
            return View("ByCategory", contents);
        }

        [HttpGet("tag/{slug}")]
        public async Task<IActionResult> ByTag(string slug)
        {
            var tag = await _context.Tags
                .Where(t => t.Slug == slug)
                .Select(t => new { t.Id, t.Name })
                .FirstOrDefaultAsync();
            
            if (tag == null) return NotFound();

            var contents = await (from ct in _context.ContentTags
                                join c in _context.Contents on ct.ContentId equals c.Id
                                join a in _context.Users on c.AuthorId equals a.Id
                                where ct.TagId == tag.Id
                                orderby c.CreatedAt descending
                                select new Content
                                {
                                    Id = c.Id,
                                    Title = c.Title,
                                    Slug = c.Slug,
                                    Body = c.Body,
                                    ThumbnailUrl = c.ThumbnailUrl,
                                    AuthorId = c.AuthorId,
                                    Category = c.Category,
                                    CreatedAt = c.CreatedAt,
                                    ViewCount = c.ViewCount,
                                    Author = new ApplicationUser
                                    {
                                        Id = a.Id,
                                        FirstName = a.FirstName,
                                        LastName = a.LastName,
                                        UserName = a.UserName
                                    }
                                }).ToListAsync();

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