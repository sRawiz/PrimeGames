using Microsoft.AspNetCore.Mvc;
using cleanNETCoreMVC.Models;
using System.Linq;
using PrimeGames.Data;

namespace cleanNETCoreMVC.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(string query)
        {
            var results = string.IsNullOrWhiteSpace(query)
                ? new List<Content>()
                : _context.Contents
                    .Where(c => c.Title.Contains(query) || c.Body.Contains(query))
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

            var resultIds = results.Select(r => r.Id).ToList();
            var suggestNews = _context.Contents
                .Where(c => !resultIds.Contains(c.Id))
                .OrderByDescending(c => c.CreatedAt)
                .Take(3)
                .ToList();

            var vm = new SearchResultViewModel
            {
                Query = query ?? string.Empty,
                Results = results,
                SuggestNews = suggestNews
            };
            return View(vm);
        }
    }
} 