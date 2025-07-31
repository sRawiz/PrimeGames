using cleanNETCoreMVC.Models;
using cleanNETCoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrimeGames.Data;
using PrimeGames.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace cleanNETCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContentController> _logger;

        public ContentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IBlobStorageService blobStorageService,
            IConfiguration configuration,
            ILogger<ContentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string search, string category, string tag, string dateFrom, string dateTo, int page = 1)
        {
            int pageSize = 10;
            if (page < 1) page = 1;
            

            
            var query = _context.Contents
                .Include(c => c.Author)
                .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ContentCategory>(category, out var selectedCategory))
            {
                query = query.Where(c => c.Category == selectedCategory);
            }

            // Apply tag filter
            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(c => c.ContentTags.Any(ct => ct.Tag.Name.Contains(tag)));
            }

            // Apply date filters
            if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
            {
                query = query.Where(c => c.CreatedAt.Date >= fromDate.Date);
            }

            if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var toDate))
            {
                query = query.Where(c => c.CreatedAt.Date <= toDate.Date);
            }

            // Order by latest first
            query = query.OrderByDescending(c => c.CreatedAt);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            var contents = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            

            
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.TotalItems = totalItems;
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Tag = tag;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            
            return View(contents);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var content = await _context.Contents.Include(c => c.Author).FirstOrDefaultAsync(m => m.Id == id);
            if (content == null) return NotFound();
            return View(content);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Content content, string TagNames, IFormFile ThumbnailFile)
        {
            content.AuthorId = _userManager.GetUserId(User);
            ModelState.Remove("Author");
            if (string.IsNullOrWhiteSpace(content.Slug) && !string.IsNullOrWhiteSpace(content.Title))
            {
                content.Slug = GenerateSlug(content.Title);
            }
            if (ModelState.IsValid)
            {
                content.CreatedAt = DateTime.Now;

                if (ThumbnailFile != null && ThumbnailFile.Length > 0)
                {
                    try
                    {
                        var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                        content.ThumbnailUrl = await _blobStorageService.UploadImageAsync(ThumbnailFile, containerName);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ThumbnailFile", $"Error uploading image: {ex.Message}");
                        return View(content);
                    }
                }

                var tagList = (TagNames ?? "").Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var existingTags = _context.Tags.Where(t => tagList.Contains(t.Name)).ToList();
                var newTagNames = tagList.Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase).ToList();
                var newTags = newTagNames.Select(name => new Tag { Name = name, Slug = name.Replace(" ", "-").ToLower() }).ToList();
                _context.Tags.AddRange(newTags);
                await _context.SaveChangesAsync();

                var allTags = existingTags.Concat(newTags).ToList();
                content.ContentTags = allTags.Select(tag => new ContentTag { TagId = tag.Id, Content = content }).ToList();

                _context.Add(content);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(content);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var content = await _context.Contents.FindAsync(id);
            if (content == null) return NotFound();
            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Content content, string TagNames, IFormFile ThumbnailFile, string CurrentThumbnailUrl)
        {
            if (id != content.Id) return NotFound();
            content.AuthorId = _userManager.GetUserId(User);
            ModelState.Remove("Author");
            if (string.IsNullOrWhiteSpace(content.Slug) && !string.IsNullOrWhiteSpace(content.Title))
            {
                content.Slug = GenerateSlug(content.Title);
            }
            if (ModelState.IsValid)
            {
                var dbContent = await _context.Contents.Include(c => c.ContentTags).FirstOrDefaultAsync(c => c.Id == id);
                if (dbContent == null) return NotFound();

                dbContent.Title = content.Title;
                dbContent.Body = content.Body;
                dbContent.Slug = content.Slug;
                dbContent.Category = content.Category;
                dbContent.UpdatedAt = DateTime.Now;

                if (ThumbnailFile != null && ThumbnailFile.Length > 0)
                {
                    try
                    {
                        var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";

                        var newImageUrl = await _blobStorageService.UploadImageAsync(ThumbnailFile, containerName);

                        if (!string.IsNullOrEmpty(CurrentThumbnailUrl))
                        {
                            await _blobStorageService.DeleteImageAsync(CurrentThumbnailUrl, containerName);
                        }

                        dbContent.ThumbnailUrl = newImageUrl;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ThumbnailFile", $"Error uploading image: {ex.Message}");
                        return View(content);
                    }
                }
                else if (!string.IsNullOrEmpty(CurrentThumbnailUrl))
                {
                    dbContent.ThumbnailUrl = CurrentThumbnailUrl;
                }

                var tagList = (TagNames ?? "").Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var existingTags = _context.Tags.Where(t => tagList.Contains(t.Name)).ToList();
                var newTagNames = tagList.Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase).ToList();
                var newTags = newTagNames.Select(name => new Tag { Name = name, Slug = name.Replace(" ", "-").ToLower() }).ToList();
                _context.Tags.AddRange(newTags);
                await _context.SaveChangesAsync();
                var allTags = existingTags.Concat(newTags).ToList();
                dbContent.ContentTags.Clear();
                foreach (var tag in allTags)
                {
                    dbContent.ContentTags.Add(new ContentTag { TagId = tag.Id, ContentId = dbContent.Id });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(content);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var content = await _context.Contents.Include(c => c.Author).FirstOrDefaultAsync(m => m.Id == id);
            if (content == null) return NotFound();
            return View(content);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var content = await _context.Contents.FindAsync(id);
            if (content != null)
            {
                if (!string.IsNullOrEmpty(content.ThumbnailUrl))
                {
                    var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                    await _blobStorageService.DeleteImageAsync(content.ThumbnailUrl, containerName);
                }

                _context.Contents.Remove(content);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CreateModal()
        {
            var model = new Content();
            return PartialView("_CreatePartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> EditModal(int id)
        {
            var model = await _context.Contents
                .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (model == null) return NotFound();
            return PartialView("_EditPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> ReadModal(int id)
        {
            var model = await _context.Contents.Include(c => c.Author).Include(c => c.ContentTags).ThenInclude(ct => ct.Tag).FirstOrDefaultAsync(c => c.Id == id);
            if (model == null) return NotFound();
            return PartialView("_ReadPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteModal(int id)
        {
            var model = await _context.Contents.FirstOrDefaultAsync(c => c.Id == id);
            if (model == null) return NotFound();
            return PartialView("_DeletePartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax(Content content, string TagNames, IFormFile ThumbnailFile)
        {
            content.AuthorId = _userManager.GetUserId(User);
            ModelState.Remove("Author");
            if (string.IsNullOrWhiteSpace(content.Slug) && !string.IsNullOrWhiteSpace(content.Title))
            {
                content.Slug = GenerateSlug(content.Title);
            }
            if (ModelState.IsValid)
            {
                content.CreatedAt = DateTime.Now;

                if (ThumbnailFile != null && ThumbnailFile.Length > 0)
                {
                    try
                    {
                        var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                        content.ThumbnailUrl = await _blobStorageService.UploadImageAsync(ThumbnailFile, containerName);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, errors = new[] { $"Error uploading image: {ex.Message}" } });
                    }
                }

                var tagList = (TagNames ?? "").Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var existingTags = _context.Tags.Where(t => tagList.Contains(t.Name)).ToList();
                var newTagNames = tagList.Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase).ToList();
                var newTags = newTagNames.Select(name => new Tag { Name = name, Slug = name.Replace(" ", "-").ToLower() }).ToList();
                _context.Tags.AddRange(newTags);
                await _context.SaveChangesAsync();

                var allTags = existingTags.Concat(newTags).ToList();
                content.ContentTags = allTags.Select(tag => new ContentTag { TagId = tag.Id, Content = content }).ToList();

                _context.Add(content);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return Json(new { success = false, errors, debugTitle = content.Title, debugBody = content.Body });
        }

        [HttpPost]
        public async Task<IActionResult> EditAjax(int id, Content content, string TagNames, IFormFile ThumbnailFile, string CurrentThumbnailUrl)
        {
            content.AuthorId = _userManager.GetUserId(User);
            ModelState.Remove("Author");
            ModelState.Remove("ThumbnailFile");
            if (string.IsNullOrWhiteSpace(content.Slug) && !string.IsNullOrWhiteSpace(content.Title))
            {
                content.Slug = GenerateSlug(content.Title);
            }
            if (id != content.Id) return Json(new { success = false, error = "Not found" });
            if (ModelState.IsValid)
            {
                var dbContent = await _context.Contents.Include(c => c.ContentTags).FirstOrDefaultAsync(c => c.Id == id);
                if (dbContent == null) return Json(new { success = false, error = "Not found" });
                dbContent.Title = content.Title;
                dbContent.Body = content.Body;
                dbContent.Slug = content.Slug;
                dbContent.Category = content.Category;
                dbContent.UpdatedAt = DateTime.Now;

                if (ThumbnailFile != null && ThumbnailFile.Length > 0)
                {
                    try
                    {
                        var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";

                        var newImageUrl = await _blobStorageService.UploadImageAsync(ThumbnailFile, containerName);

                        if (!string.IsNullOrEmpty(CurrentThumbnailUrl))
                        {
                            await _blobStorageService.DeleteImageAsync(CurrentThumbnailUrl, containerName);
                        }

                        dbContent.ThumbnailUrl = newImageUrl;
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, errors = new[] { $"Error uploading image: {ex.Message}" } });
                    }
                }
                else if (!string.IsNullOrEmpty(CurrentThumbnailUrl))
                {
                    dbContent.ThumbnailUrl = CurrentThumbnailUrl;
                }

                var tagList = (TagNames ?? "").Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var existingTags = _context.Tags.Where(t => tagList.Contains(t.Name)).ToList();
                var newTagNames = tagList.Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase).ToList();
                var newTags = newTagNames.Select(name => new Tag { Name = name, Slug = name.Replace(" ", "-").ToLower() }).ToList();
                _context.Tags.AddRange(newTags);
                await _context.SaveChangesAsync();
                var allTags = existingTags.Concat(newTags).ToList();
                dbContent.ContentTags.Clear();
                foreach (var tag in allTags)
                {
                    dbContent.ContentTags.Add(new ContentTag { TagId = tag.Id, ContentId = dbContent.Id });
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray() });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var content = await _context.Contents.FindAsync(id);
            if (content != null)
            {
                if (!string.IsNullOrEmpty(content.ThumbnailUrl))
                {
                    var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                    await _blobStorageService.DeleteImageAsync(content.ThumbnailUrl, containerName);
                }

                _context.Contents.Remove(content);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, error = "Not found" });
        }

        private string GenerateSlug(string title)
        {
            var slug = title.ToLowerInvariant();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9ก-๙\s-]", "");
            var words = slug.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Take(5);
            slug = string.Join("-", words);
            if (slug.Length > 50)
                slug = slug.Substring(0, 50).Trim('-');
            slug = System.Text.RegularExpressions.Regex.Replace(slug, "-+", "-");
            slug = slug.Trim('-');
            return slug;
        }
    }
}