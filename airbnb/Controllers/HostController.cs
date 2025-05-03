using airbnb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace airbnb.Controllers
{
    public class HostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HostController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Yorumlar()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));

            var yorumlar = await _context.Houses
                .Where(h => h.OwnerId == userId)
                .SelectMany(h => h.Reservations)
                .Where(r => r.Review != null)
                .Include(r => r.Review)
                .Include(r => r.House)
                .ToListAsync();

            return View(yorumlar);
        }

        public IActionResult HostStats()
        {
            // Giriş yapan kullanıcının (host'un) ID'sini al
            int hostId = int.Parse(User.FindFirst("UserId").Value);

            var stats = _context.HostStatsViewModel
                .FromSqlRaw("EXEC sp_GetHostReservationStats @p0", hostId)
                .AsEnumerable()
                .FirstOrDefault();

            return View(stats);
        }
    }
}
