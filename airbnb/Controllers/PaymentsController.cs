using airbnb.Data;
using airbnb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace airbnb.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
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

        [HttpGet]
        public async Task<IActionResult> PayNow(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.House)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null || reservation.TenantId != GetCurrentUserId())
                return NotFound();

            int totalDays = (int)(reservation.EndDate - reservation.StartDate).TotalDays;
            ViewBag.TotalPrice = reservation.House.PricePerNight * totalDays;

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayNowConfirmed(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.House)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null || reservation.TenantId != GetCurrentUserId())
                return NotFound();

            reservation.Status = "Paid";
            var payment = new Payment
            {
                ReservationId = reservation.ReservationId,
                Amount = reservation.House.PricePerNight * (reservation.EndDate - reservation.StartDate).Days,
                IsPaid = true,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ödeme başarıyla tamamlandı.";
            return RedirectToAction("MyReservations", "Reservations");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartPayment(int houseId, DateTime startDate, DateTime endDate)
        {
            TempData["HouseId"] = houseId;
            TempData["StartDate"] = startDate.ToString("yyyy-MM-dd");
            TempData["EndDate"] = endDate.ToString("yyyy-MM-dd");

            return RedirectToAction("PayNowTemp");
        }

        [HttpGet]
        public async Task<IActionResult> PayNowTemp()
        {

            if (TempData["HouseId"] == null || TempData["StartDate"] == null || TempData["EndDate"] == null)
                return RedirectToAction("Index", "Home");

            int houseId = int.Parse(TempData["HouseId"].ToString());
            DateTime start = DateTime.Parse(TempData["StartDate"].ToString());
            DateTime end = DateTime.Parse(TempData["EndDate"].ToString());

            var house = await _context.Houses.FindAsync(houseId);
            var currentUserId = GetCurrentUserId(); // bu metot seni giriş yapan kullanıcının ID'sine götürsün
            if (house == null)
                return NotFound();

            if (house.OwnerId == currentUserId)
            {
                TempData["Error"] = "Kendi evinizi kiralayamazsınız.";
                return RedirectToAction("Details", "Houses", new { id = houseId });
            }
            if (house == null) return NotFound();

            ViewBag.House = house;
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.TotalPrice = house.PricePerNight * (end - start).Days;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletePayment(int houseId, DateTime startDate, DateTime endDate)
        {
            var userId = GetCurrentUserId();

            // 1. House çek
            var house = await _context.Houses.FirstOrDefaultAsync(h => h.HouseId == houseId);
            if (house == null)
                return NotFound();

            // Müsaitlik kontrolü: Kullanıcının seçtiği tarih bu evin uygun olduğu aralıkta mı?
            bool isAvailable = await _context.HouseAvailabilities.AnyAsync(a =>
    a.HouseId == houseId &&
    startDate >= a.AvailableFrom &&
    endDate <= a.AvailableTo);

            if (!isAvailable)
            {
                TempData["Error"] = "Seçilen tarih aralığında bu ev müsait değil.";
                return RedirectToAction("Details", "Houses", new { id = houseId });
            }


            // 2. Tarih çakışması var mı kontrol et
            var overlapping = await _context.Reservations
                .Where(r => r.HouseId == houseId && r.Status != "Cancelled")
                .Where(r =>
                    (startDate >= r.StartDate && startDate < r.EndDate) ||
                    (endDate > r.StartDate && endDate <= r.EndDate) ||
                    (startDate <= r.StartDate && endDate >= r.EndDate))
                .ToListAsync();

            if (overlapping.Any())
            {
                TempData["Error"] = "Seçilen tarih aralığı bu ev için zaten rezerve edilmiş.";
                return RedirectToAction("Details", "Houses", new { id = houseId });
            }

            // 3. Rezervasyon oluştur
            var reservation = new Reservation
            {
                HouseId = houseId,
                StartDate = startDate,
                EndDate = endDate,
                TenantId = userId,
                Status = "Paid",
                CreatedAt = DateTime.Now
            };

            _context.Reservations.Add(reservation);

            // 4. Ödeme oluştur
            var payment = new Payment
            {
                Reservation = reservation,
                Amount = house.PricePerNight * (endDate - startDate).Days,
                IsPaid = true,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ödeme ve rezervasyon başarıyla tamamlandı!";
            return RedirectToAction("MyReservations", "Reservations");
        }


    }
}