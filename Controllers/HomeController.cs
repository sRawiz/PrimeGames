using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PrimeGames.Models;
using cleanNETCoreMVC.Models;
using Microsoft.EntityFrameworkCore;
using PrimeGames.Data;

namespace PrimeGames.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var contents = await _context.Contents
                .Include(c => c.Author)
                .OrderByDescending(c => c.CreatedAt)
                .Take(12)
                .ToListAsync();
            return View(contents);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
