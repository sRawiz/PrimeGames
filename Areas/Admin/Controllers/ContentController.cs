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
using System.Transactions;

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
        private readonly IImageProcessingService _imageProcessingService;

        public ContentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IBlobStorageService blobStorageService,
            IConfiguration configuration,
            ILogger<ContentController> logger,
            IImageProcessingService imageProcessingService)
        {
            _context = context;
            _userManager = userManager;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
            _logger = logger;
            _imageProcessingService = imageProcessingService;
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
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation("Admin {UserId} attempting to create content: {Title}", userId, content.Title);

            try
            {
                content.AuthorId = userId ?? throw new InvalidOperationException("User ID cannot be null");
                ModelState.Remove("Author");
                
                if (string.IsNullOrWhiteSpace(content.Slug) && !string.IsNullOrWhiteSpace(content.Title))
                {
                    content.Slug = GenerateSlug(content.Title);
                    _logger.LogDebug("Generated slug: {Slug} for title: {Title}", content.Slug, content.Title);
                }

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    _logger.LogWarning("Create content validation failed for {Title}. Errors: {Errors}", content.Title, errors);
                    return View(content);
                }

                content.CreatedAt = DateTime.Now;

                string? uploadedImageUrl = null;
                try
                {
                    uploadedImageUrl = await ProcessImageUploadAsync(ThumbnailFile, "content creation");
                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                    {
                        content.ThumbnailUrl = uploadedImageUrl;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process image upload for content: {Title}", content.Title);
                    ModelState.AddModelError("ThumbnailFile", $"เกิดข้อผิดพลาดในการประมวลผลรูปภาพ: {ex.Message}");
                    return View(content);
                }

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    _logger.LogDebug("Starting database transaction for content creation: {Title}", content.Title);

                    _logger.LogDebug("Processing tags for content: {Title}", content.Title);

                    var allTags = await ProcessTagsAsync(TagNames);
                    content.ContentTags = allTags.Select(tag => new ContentTag { TagId = tag.Id, Content = content }).ToList();

                    _context.Add(content);
                    await _context.SaveChangesAsync();

                    transactionScope.Complete();
                    _logger.LogInformation("Successfully created content: {Title} with ID: {ContentId} by user: {UserId}", 
                        content.Title, content.Id, userId);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database transaction failed while creating content: {Title} by user: {UserId}", 
                        content.Title, userId);

                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                    {
                        try
                        {
                            var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                            await _blobStorageService.DeleteImageAsync(uploadedImageUrl, containerName);
                            _logger.LogInformation("Cleaned up uploaded image after database failure: {Url}", uploadedImageUrl);
                        }
                        catch (Exception cleanupEx)
                        {
                            _logger.LogError(cleanupEx, "Failed to cleanup uploaded image: {Url}", uploadedImageUrl);
                        }
                    }

                    ModelState.AddModelError("", "เกิดข้อผิดพลาดในการบันทึกข้อมูล กรุณาลองใหม่อีกครั้ง");
                    return View(content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating content: {Title} by user: {UserId}", 
                    content.Title ?? "Unknown", userId);
                ModelState.AddModelError("", "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองใหม่อีกครั้ง");
                return View(content);
            }
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
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation("Admin {UserId} attempting to edit content ID: {ContentId}, Title: {Title}", 
                userId, id, content.Title);

            if (id != content.Id)
            {
                _logger.LogWarning("Content ID mismatch in edit request. URL ID: {UrlId}, Form ID: {FormId} by user: {UserId}", 
                    id, content.Id, userId);
                return NotFound();
            }

            try
            {
                content.AuthorId = userId ?? throw new InvalidOperationException("User ID cannot be null");
                ModelState.Remove("Author");
                
                if (string.IsNullOrWhiteSpace(content.Slug) && !string.IsNullOrWhiteSpace(content.Title))
                {
                    content.Slug = GenerateSlug(content.Title);
                    _logger.LogDebug("Generated new slug: {Slug} for title: {Title}", content.Slug, content.Title);
                }

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    _logger.LogWarning("Edit content validation failed for ID: {ContentId}. Errors: {Errors}", id, errors);
                    return View(content);
                }

                var dbContent = await _context.Contents.Include(c => c.ContentTags).FirstOrDefaultAsync(c => c.Id == id);
                if (dbContent == null)
                {
                    _logger.LogWarning("Content not found for edit. ID: {ContentId} by user: {UserId}", id, userId);
                    return NotFound();
                }

                string? newImageUrl = null;
                string? oldImageUrl = CurrentThumbnailUrl;

                try
                {
                    newImageUrl = await ProcessImageUploadAsync(ThumbnailFile, $"content edit ID: {id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process image upload for content ID: {ContentId}", id);
                    ModelState.AddModelError("ThumbnailFile", $"เกิดข้อผิดพลาดในการประมวลผลรูปภาพ: {ex.Message}");
                    return View(content);
                }

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    _logger.LogDebug("Starting database transaction for content edit ID: {ContentId}", id);

                    dbContent.Title = content.Title;
                    dbContent.Body = content.Body;
                    dbContent.Slug = content.Slug;
                    dbContent.Category = content.Category;
                    dbContent.UpdatedAt = DateTime.Now;

                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        dbContent.ThumbnailUrl = newImageUrl;
                    }
                    else if (!string.IsNullOrEmpty(CurrentThumbnailUrl))
                    {
                        dbContent.ThumbnailUrl = CurrentThumbnailUrl;
                    }

                    _logger.LogDebug("Processing tags for content ID: {ContentId}", id);

                    var allTags = await ProcessTagsAsync(TagNames);
                    dbContent.ContentTags.Clear();
                    foreach (var tag in allTags)
                    {
                        dbContent.ContentTags.Add(new ContentTag { TagId = tag.Id, ContentId = dbContent.Id });
                    }

                    await _context.SaveChangesAsync();

                    transactionScope.Complete();
                    _logger.LogInformation("Successfully updated content ID: {ContentId}, Title: {Title} by user: {UserId}", 
                        id, content.Title, userId);

                    if (!string.IsNullOrEmpty(newImageUrl) && !string.IsNullOrEmpty(oldImageUrl))
                    {
                        try
                        {
                            var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                            await _blobStorageService.DeleteImageAsync(oldImageUrl, containerName);
                            _logger.LogInformation("Deleted old thumbnail: {Url} for content ID: {ContentId}", oldImageUrl, id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to delete old thumbnail: {Url} for content ID: {ContentId}", oldImageUrl, id);
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database transaction failed while editing content ID: {ContentId} by user: {UserId}", 
                        id, userId);

                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        try
                        {
                            var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                            await _blobStorageService.DeleteImageAsync(newImageUrl, containerName);
                            _logger.LogInformation("Cleaned up new image after database failure: {Url}", newImageUrl);
                        }
                        catch (Exception cleanupEx)
                        {
                            _logger.LogError(cleanupEx, "Failed to cleanup new image: {Url}", newImageUrl);
                        }
                    }

                    ModelState.AddModelError("", "เกิดข้อผิดพลาดในการแก้ไขข้อมูล กรุณาลองใหม่อีกครั้ง");
                    return View(content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while editing content ID: {ContentId} by user: {UserId}", 
                    id, userId);
                ModelState.AddModelError("", "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองใหม่อีกครั้ง");
                return View(content);
            }
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
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation("Admin {UserId} attempting to delete content ID: {ContentId}", userId, id);

            try
            {
                var content = await _context.Contents.FindAsync(id);
                if (content == null)
                {
                    _logger.LogWarning("Content not found for deletion. ID: {ContentId} by user: {UserId}", id, userId);
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Found content to delete: ID: {ContentId}, Title: {Title}", id, content.Title);
                
                string? imageUrlToDelete = content.ThumbnailUrl;

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    _logger.LogDebug("Starting database transaction for content deletion ID: {ContentId}", id);

                    _context.Contents.Remove(content);
                    await _context.SaveChangesAsync();

                    transactionScope.Complete();
                    _logger.LogInformation("Successfully deleted content from database: ID: {ContentId}, Title: {Title} by user: {UserId}", 
                        id, content.Title, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database transaction failed while deleting content ID: {ContentId} by user: {UserId}", 
                        id, userId);
                    throw;
                }

                if (!string.IsNullOrEmpty(imageUrlToDelete))
                {
                    try
                    {
                        var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                        await _blobStorageService.DeleteImageAsync(imageUrlToDelete, containerName);
                        _logger.LogInformation("Successfully deleted thumbnail: {Url} for content ID: {ContentId}", 
                            imageUrlToDelete, id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete thumbnail: {Url} for content ID: {ContentId}. " +
                            "Database deletion was successful but image cleanup failed.", imageUrlToDelete, id);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting content ID: {ContentId} by user: {UserId}", 
                    id, userId);
                
                
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการลบข้อมูล กรุณาลองใหม่อีกครั้ง";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult CreateModal()
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
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation("Admin {UserId} attempting AJAX create content: {Title}", userId, content.Title);

            try
            {
                content.AuthorId = userId ?? throw new InvalidOperationException("User ID cannot be null");
                ModelState.Remove("Author");
                
                if (string.IsNullOrWhiteSpace(content.Slug) && !string.IsNullOrWhiteSpace(content.Title))
                {
                    content.Slug = GenerateSlug(content.Title);
                    _logger.LogDebug("Generated slug for AJAX create: {Slug}", content.Slug);
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                    _logger.LogWarning("AJAX create validation failed for {Title}. Errors: {Errors}", 
                        content.Title, string.Join("; ", errors));
                    return Json(new { success = false, errors, debugTitle = content.Title, debugBody = content.Body });
                }

                content.CreatedAt = DateTime.Now;

                string? uploadedImageUrl = null;
                try
                {
                    uploadedImageUrl = await ProcessImageUploadAsync(ThumbnailFile, "AJAX content creation");
                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                    {
                        content.ThumbnailUrl = uploadedImageUrl;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AJAX image processing failed for {Title}", content.Title);
                    return Json(new { success = false, errors = new[] { $"เกิดข้อผิดพลาดในการประมวลผลรูปภาพ: {ex.Message}" } });
                }

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    _logger.LogDebug("Starting AJAX database transaction for content: {Title}", content.Title);

                    var allTags = await ProcessTagsAsync(TagNames);
                    content.ContentTags = allTags.Select(tag => new ContentTag { TagId = tag.Id, Content = content }).ToList();

                    _context.Add(content);
                    await _context.SaveChangesAsync();

                    transactionScope.Complete();
                    _logger.LogInformation("AJAX successfully created content: {Title} with ID: {ContentId}", 
                        content.Title, content.Id);

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AJAX database transaction failed for content: {Title}", content.Title);

                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                    {
                        try
                        {
                            var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                            await _blobStorageService.DeleteImageAsync(uploadedImageUrl, containerName);
                            _logger.LogInformation("AJAX cleaned up image after database failure: {Url}", uploadedImageUrl);
                        }
                        catch (Exception cleanupEx)
                        {
                            _logger.LogError(cleanupEx, "AJAX failed to cleanup image: {Url}", uploadedImageUrl);
                        }
                    }

                    return Json(new { success = false, errors = new[] { "เกิดข้อผิดพลาดในการบันทึกข้อมูล" } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AJAX unexpected error creating content: {Title} by user: {UserId}", 
                    content.Title ?? "Unknown", userId);
                return Json(new { success = false, errors = new[] { "เกิดข้อผิดพลาดที่ไม่คาดคิด" } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditAjax(int id, Content content, string TagNames, IFormFile ThumbnailFile, string CurrentThumbnailUrl)
        {
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation("Admin {UserId} attempting AJAX edit content ID: {ContentId}, Title: {Title}", 
                userId, id, content.Title);

            try
            {
                content.AuthorId = userId ?? throw new InvalidOperationException("User ID cannot be null");
                ModelState.Remove("Author");
                ModelState.Remove("ThumbnailFile");
                
                if (string.IsNullOrWhiteSpace(content.Slug) && !string.IsNullOrWhiteSpace(content.Title))
                {
                    content.Slug = GenerateSlug(content.Title);
                    _logger.LogDebug("Generated new slug for AJAX edit: {Slug} for title: {Title}", content.Slug, content.Title);
                }

                if (id != content.Id)
                {
                    _logger.LogWarning("AJAX Content ID mismatch in edit request. URL ID: {UrlId}, Form ID: {FormId} by user: {UserId}", 
                        id, content.Id, userId);
                    return Json(new { success = false, error = "Not found" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                    _logger.LogWarning("AJAX edit content validation failed for ID: {ContentId}. Errors: {Errors}", id, string.Join("; ", errors));
                    return Json(new { success = false, errors });
                }

                var dbContent = await _context.Contents.Include(c => c.ContentTags).FirstOrDefaultAsync(c => c.Id == id);
                if (dbContent == null)
                {
                    _logger.LogWarning("AJAX Content not found for edit. ID: {ContentId} by user: {UserId}", id, userId);
                    return Json(new { success = false, error = "Not found" });
                }

                string? newImageUrl = null;
                string? oldImageUrl = CurrentThumbnailUrl;

                try
                {
                    newImageUrl = await ProcessImageUploadAsync(ThumbnailFile, $"AJAX content edit ID: {id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AJAX image processing failed for content ID: {ContentId}", id);
                    return Json(new { success = false, errors = new[] { $"เกิดข้อผิดพลาดในการประมวลผลรูปภาพ: {ex.Message}" } });
                }

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    _logger.LogDebug("Starting AJAX database transaction for content edit ID: {ContentId}", id);

                    dbContent.Title = content.Title;
                    dbContent.Body = content.Body;
                    dbContent.Slug = content.Slug;
                    dbContent.Category = content.Category;
                    dbContent.UpdatedAt = DateTime.Now;

                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        dbContent.ThumbnailUrl = newImageUrl;
                    }
                    else if (!string.IsNullOrEmpty(CurrentThumbnailUrl))
                    {
                        dbContent.ThumbnailUrl = CurrentThumbnailUrl;
                    }

                    _logger.LogDebug("AJAX processing tags for content ID: {ContentId}", id);

                    var allTags = await ProcessTagsAsync(TagNames);
                    dbContent.ContentTags.Clear();
                    foreach (var tag in allTags)
                    {
                        dbContent.ContentTags.Add(new ContentTag { TagId = tag.Id, ContentId = dbContent.Id });
                    }

                    await _context.SaveChangesAsync();

                    transactionScope.Complete();
                    _logger.LogInformation("AJAX successfully updated content ID: {ContentId}, Title: {Title} by user: {UserId}", 
                        id, content.Title, userId);

                    if (!string.IsNullOrEmpty(newImageUrl) && !string.IsNullOrEmpty(oldImageUrl))
                    {
                        try
                        {
                            var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                            await _blobStorageService.DeleteImageAsync(oldImageUrl, containerName);
                            _logger.LogInformation("AJAX deleted old thumbnail: {Url} for content ID: {ContentId}", oldImageUrl, id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "AJAX failed to delete old thumbnail: {Url} for content ID: {ContentId}", oldImageUrl, id);
                        }
                    }

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AJAX database transaction failed while editing content ID: {ContentId} by user: {UserId}", 
                        id, userId);

                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        try
                        {
                            var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                            await _blobStorageService.DeleteImageAsync(newImageUrl, containerName);
                            _logger.LogInformation("AJAX cleaned up new image after database failure: {Url}", newImageUrl);
                        }
                        catch (Exception cleanupEx)
                        {
                            _logger.LogError(cleanupEx, "AJAX failed to cleanup new image: {Url}", newImageUrl);
                        }
                    }

                    return Json(new { success = false, errors = new[] { "เกิดข้อผิดพลาดในการแก้ไขข้อมูล" } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AJAX unexpected error while editing content ID: {ContentId} by user: {UserId}", 
                    id, userId);
                return Json(new { success = false, errors = new[] { "เกิดข้อผิดพลาดที่ไม่คาดคิด" } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation("Admin {UserId} attempting AJAX delete content ID: {ContentId}", userId, id);

            try
            {
                var content = await _context.Contents.FindAsync(id);
                if (content == null)
                {
                    _logger.LogWarning("AJAX content not found for deletion. ID: {ContentId} by user: {UserId}", id, userId);
                    return Json(new { success = false, error = "Content not found" });
                }

                _logger.LogInformation("AJAX found content to delete: ID: {ContentId}, Title: {Title}", id, content.Title);
                
                string? imageUrlToDelete = content.ThumbnailUrl;

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    _logger.LogDebug("Starting AJAX database transaction for content deletion ID: {ContentId}", id);

                    _context.Contents.Remove(content);
                    await _context.SaveChangesAsync();

                    transactionScope.Complete();
                    _logger.LogInformation("AJAX successfully deleted content from database: ID: {ContentId}, Title: {Title}", 
                        id, content.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AJAX database transaction failed while deleting content ID: {ContentId}", id);
                    return Json(new { success = false, error = "Database error occurred" });
                }

                if (!string.IsNullOrEmpty(imageUrlToDelete))
                {
                    try
                    {
                        var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                        await _blobStorageService.DeleteImageAsync(imageUrlToDelete, containerName);
                        _logger.LogInformation("AJAX successfully deleted thumbnail: {Url} for content ID: {ContentId}", 
                            imageUrlToDelete, id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "AJAX failed to delete thumbnail: {Url} for content ID: {ContentId}. " +
                            "Database deletion was successful but image cleanup failed.", imageUrlToDelete, id);
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AJAX unexpected error while deleting content ID: {ContentId} by user: {UserId}", 
                    id, userId);
                return Json(new { success = false, error = "Unexpected error occurred" });
            }
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

        private async Task<List<Tag>> ProcessTagsAsync(string tagNames)
        {
            var tagList = (tagNames ?? "").Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!tagList.Any()) return new List<Tag>();

            var existingTags = await _context.Tags.Where(t => tagList.Contains(t.Name)).ToListAsync();
            var newTagNames = tagList.Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase).ToList();
            
            List<Tag> newTags = new();
            if (newTagNames.Any())
            {
                newTags = newTagNames.Select(name => new Tag { Name = name, Slug = name.Replace(" ", "-").ToLower() }).ToList();
                _context.Tags.AddRange(newTags);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created {NewTagCount} new tags: {TagNames}", newTags.Count, string.Join(", ", newTagNames));
            }

            return existingTags.Concat(newTags).ToList();
        }

        private async Task<string?> ProcessImageUploadAsync(IFormFile? imageFile, string operation)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            try
            {
                _logger.LogInformation("Processing image upload for {Operation}: {FileName}, Size: {FileSize} bytes", 
                    operation, imageFile.FileName, imageFile.Length);

                if (!_imageProcessingService.IsValidImageFile(imageFile))
                {
                    throw new InvalidOperationException("ไฟล์ที่อัปโหลดไม่ใช่รูปภาพที่รองรับ (รองรับเฉพาะ JPG, PNG, WebP)");
                }

                const long maxFileSizeBytes = 1024 * 1024; // 1MB

                IFormFile processedImageFile = imageFile;

                if (imageFile.Length > maxFileSizeBytes)
                {
                    _logger.LogInformation("Image size ({FileSize} bytes) exceeds limit. Starting resize and compression...", 
                        imageFile.Length);

                    var resizedImageBytes = await _imageProcessingService.ResizeAndCompressImageAsync(
                        imageFile, 
                        maxFileSizeBytes, 
                        maxWidth: 1920, 
                        maxHeight: 1080
                    );

                    var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + "_resized" + Path.GetExtension(imageFile.FileName);
                    processedImageFile = _imageProcessingService.BytesToFormFile(resizedImageBytes, fileName, imageFile.ContentType);

                    _logger.LogInformation("Image successfully resized from {OriginalSize} to {NewSize} bytes", 
                        imageFile.Length, resizedImageBytes.Length);
                }
                else
                {
                    _logger.LogInformation("Image size is within limits, uploading without processing");
                }

                var containerName = _configuration["AzureStorage:ContainerName"] ?? "thumbnail-images";
                var uploadedImageUrl = await _blobStorageService.UploadImageAsync(processedImageFile, containerName);

                _logger.LogInformation("Successfully uploaded processed image: {Url}", uploadedImageUrl);
                return uploadedImageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process image upload for {Operation}: {FileName}", 
                    operation, imageFile?.FileName ?? "unknown");
                throw;
            }
        }
    }
}