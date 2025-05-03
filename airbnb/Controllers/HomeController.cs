using System.Diagnostics;
using airbnb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using airbnb.Data;

namespace airbnb.Controllers
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
            // Sadece aktif olan evler
            var activeHouses = await _context.Houses
                .Where(h => h.IsActive)
                .Include(h => h.Owner)
                .Include(h => h.Reservations)         // Rezervasyonları dahil et
                    .ThenInclude(r => r.Review)       // Her rezervasyonun yorumlarını da dahil et
                .ToListAsync();

            var populerEvler = _context.TopReservedHouseViewModel
    .FromSqlRaw("EXEC sp_GetTop5ReservedHouses")
    .ToList();

            ViewBag.PopulerEvler = populerEvler;




            return View(activeHouses);
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
