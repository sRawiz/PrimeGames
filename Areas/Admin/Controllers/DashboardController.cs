using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrimeGames.Data;
using cleanNETCoreMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace cleanNETCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string startDate, string endDate, string range)
        {
            var totalNews = await _context.Contents.CountAsync();
            var newsByCategory = await _context.Contents
                .GroupBy(c => c.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();
            var today = DateTime.Today;
            var newsToday = await _context.Contents.CountAsync(c => c.CreatedAt.Date == today);
            var totalUsers = await _userManager.Users.CountAsync();
            var usersToday = await _userManager.Users.CountAsync(u => u.CreatedAt.Date == today);
            var totalFavorites = await _context.Favorites.CountAsync();
            var totalTags = await _context.Tags.Select(t => t.Name).Distinct().CountAsync();
            ViewBag.TotalNews = totalNews;
            ViewBag.NewsByCategory = newsByCategory;
            ViewBag.NewsToday = newsToday;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.UsersToday = usersToday;
            ViewBag.TotalFavorites = totalFavorites;
            ViewBag.TotalTags = totalTags;
            var now = DateTime.Now;
            var months = Enumerable.Range(0, 12)
                .Select(i => new DateTime(now.Year, now.Month, 1).AddMonths(-i))
                .OrderBy(d => d)
                .ToList();
            var newsByMonth = await _context.Contents
                .Where(c => c.CreatedAt >= months.First())
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToListAsync();
            var chartData = months.Select(m => {
                var found = newsByMonth.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);
                return new { Month = m.ToString("yyyy-MM"), Count = found?.Count ?? 0 };
            }).ToList();
            ViewBag.ChartData = chartData;
            DateTime? start = null, end = null;
            if (!string.IsNullOrEmpty(range))
            {
                if (range == "7")
                {
                    start = DateTime.Today.AddDays(-6);
                    end = DateTime.Today;
                }
                else if (range == "30")
                {
                    start = DateTime.Today.AddDays(-29);
                    end = DateTime.Today;
                }
                else if (range == "year")
                {
                    start = new DateTime(DateTime.Today.Year, 1, 1);
                    end = DateTime.Today;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var s)) start = s.Date;
                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var e)) end = e.Date;
            }
            var topNewsQuery = _context.Contents.AsQueryable();
            if (start.HasValue) topNewsQuery = topNewsQuery.Where(c => c.CreatedAt.Date >= start.Value);
            if (end.HasValue) topNewsQuery = topNewsQuery.Where(c => c.CreatedAt.Date <= end.Value);
            var topNews = await topNewsQuery
                .OrderByDescending(c => c.ViewCount)
                .ThenByDescending(c => c.CreatedAt)
                .Take(10)
                .Select(c => new { c.Id, c.Title, c.Slug, c.ViewCount, c.Category, c.CreatedAt })
                .ToListAsync();
            ViewBag.TopNews = topNews;
            ViewBag.StartDate = start?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end?.ToString("yyyy-MM-dd");
            ViewBag.Range = range;
            var latestNews = await _context.Contents
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new { c.Id, c.Title, c.Slug, c.Category, c.CreatedAt })
                .ToListAsync();
            ViewBag.LatestNews = latestNews;
            return View();
        }
    }
} 