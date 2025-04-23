using airbnb.Data;
using airbnb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace airbnb.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }


        private int GetCurrentUserId()
        {

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                throw new Exception("Giriş yapmanız gerekmektedir.");
            return int.Parse(userIdClaim.Value);
        }

        // GET: Reservations/Create?houseId=5
        [HttpGet]
        public async Task<IActionResult> Create(int houseId)
        {
            var house = await _context.Houses.FindAsync(houseId);
            if (house == null) return NotFound();

            var reservation = new Reservation
            {
                HouseId = houseId,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            // rezerve edilmiş tarih aralıkları
            var reservedRanges = await _context.Reservations
                .Where(r => r.HouseId == houseId && r.Status == "Pending")
                .Select(r => new { r.StartDate, r.EndDate })
                .ToListAsync();

            // evin müsaitlik dönemi
            var availability = await _context.HouseAvailabilities
                .Where(a => a.HouseId == houseId)
                .FirstOrDefaultAsync(); // varsayım: tek bir aralık

            ViewData["House"] = house;
            ViewData["ReservedRanges"] = reservedRanges;
            ViewData["AvailabilityStart"] = availability?.AvailableFrom.ToString("yyyy-MM-dd");
            ViewData["AvailabilityEnd"] = availability?.AvailableTo.ToString("yyyy-MM-dd");

            return View(reservation);
        }
















        // Kullanıcının kendi rezervasyonlarını listeler
        public async Task<IActionResult> MyReservations()
        {
            var currentUserId = GetCurrentUserId();

            var reservations = await _context.Reservations
                .Include(r => r.House)
                .Where(r => r.TenantId == currentUserId)
                .ToListAsync();

            return View(reservations);
        }
        [HttpGet]
        public async Task<IActionResult> Pay(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.House)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null || reservation.TenantId != GetCurrentUserId())
                return NotFound();

            // Gün sayısını ve toplam fiyatı hesapla
            int totalDays = (int)(reservation.EndDate - reservation.StartDate).TotalDays;
            ViewBag.TotalPrice = reservation.House.PricePerNight * totalDays;

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayConfirmed(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.House)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null || reservation.TenantId != GetCurrentUserId())
                return NotFound();

            reservation.Status = "Paid";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezervasyon ve ödeme işlemi başarıyla tamamlandı.";
            return RedirectToAction("MyReservations");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (reservation.TenantId != currentUserId)
                return Forbid();

            // Durum ne olursa olsun iptal et
            reservation.Status = "Cancelled";

            await _context.SaveChangesAsync();

            // Kullanıcıya mesaj ver
            if (reservation.Status == "Paid")
            {
                TempData["Success"] = "Rezervasyon iptal edildi. İade işleminiz 3 iş günü içinde gerçekleştirilecektir.";
            }
            else
            {
                TempData["Success"] = "Rezervasyon başarıyla iptal edildi.";
            }

            return RedirectToAction("MyReservations");
        }


    }
}
