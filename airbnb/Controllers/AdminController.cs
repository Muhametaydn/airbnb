using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using airbnb.Data; // AppDbContext'in bulunduğu namespace (seninkini kontrol et)
using airbnb.Models; // ViewModel burada olabilir
using System.Linq;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        var now = DateTime.Now;
        var oneWeekAgo = now.AddDays(-7);

        // Toplam sayılar
        var totalUsers = _context.Users.Count();
        var totalHouses = _context.Houses.Count();
        var totalReservations = _context.Reservations.Count();

        // Aktif kullanıcılar
        var activeUsers = _context.Users.Count(u => u.IsActive);
        ViewBag.ActiveUsers = activeUsers;

        // Günlük rezervasyon sayıları (son 7 gün)
        var dailyData = _context.Reservations
     .Where(r => r.CreatedAt >= oneWeekAgo)
     .AsEnumerable() // EF sorgusunu bitir, belleğe al
     .GroupBy(r => r.CreatedAt.Date)
     .Select(g => new
     {
         Date = g.Key.ToString("yyyy-MM-dd"),
         Count = g.Count()
     })
     .OrderBy(x => x.Date)
     .ToList();

        ViewBag.ReservationDates = dailyData.Select(d => d.Date).ToList();
        ViewBag.ReservationCounts = dailyData.Select(d => d.Count).ToList();

        // Ödeme bilgileri
        var totalPaid = _context.Payments
            .Where(p => p.IsPaid)
            .Sum(p => (decimal?)p.Amount) ?? 0;

        var totalCancelled = _context.Payments
            .Where(p => !p.IsPaid)
            .Sum(p => (decimal?)p.Amount) ?? 0;

        var totalAll = _context.Payments.Sum(p => (decimal?)p.Amount) ?? 0;
        var profit = totalAll - totalCancelled;

        // ViewModel oluştur
        var viewModel = new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalHouses = totalHouses,
            TotalReservations = totalReservations,
            TotalPayments = totalAll,
            CancelledPayments = totalCancelled,
            Profit = profit
        };

        return View(viewModel);
    }

    public IActionResult ListHouses()
    {
        var houses = _context.Houses.Include(h => h.Owner).ToList();
        return View(houses);
    }
    public IActionResult ToggleStatus(int id)
    {
        var house = _context.Houses.Find(id);
        if (house == null) return NotFound();

        house.IsActive = !house.IsActive;
        _context.SaveChanges();

        return RedirectToAction("ListHouses");
    }

    public IActionResult DeleteHouse(int id)
    {
        var house = _context.Houses.Find(id);
        if (house == null) return NotFound();

        _context.Houses.Remove(house);
        _context.SaveChanges();

        return RedirectToAction("ListHouses");
    }
    [HttpGet]
    public IActionResult EditHouse(int id)
    {
        var house = _context.Houses.Find(id);
        if (house == null) return NotFound();

        return View(house);
    }

    [HttpPost]
    [HttpPost]
    public IActionResult EditHouse(House house)
    {
        if (!ModelState.IsValid)
            return View(house);

        var existing = _context.Houses.Find(house.HouseId);
        if (existing == null) return NotFound();

        existing.Title = house.Title;
        existing.Description = house.Description;
        existing.Location = house.Location;
        existing.PricePerNight = house.PricePerNight;
        existing.IsActive = house.IsActive;

        if (house.ImageFile != null)
        {
            // Yeni dosya varsa: yükle ve eski dosyanın yolunu değiştir
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/houses");
            Directory.CreateDirectory(uploadsFolder); // klasör yoksa oluştur

            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(house.ImageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                house.ImageFile.CopyTo(stream);
            }

            // Yeni yol ata (eskiyi silmiyoruz)
            existing.ImagePath = "/images/houses/" + uniqueName;
        }

        _context.SaveChanges();

        return RedirectToAction("ListHouses");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Reservations()
    {
        var reservations = _context.Reservations
            .Include(r => r.Tenant)
            .Include(r => r.House)
            .Include(r => r.Payment)
            .ToList();

        return View(reservations);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminCancelReservation(int id)
    {
        var reservation = _context.Reservations.FirstOrDefault(r => r.ReservationId == id);
        if (reservation == null) return NotFound();

        reservation.Status = "Cancelled";

        var payment = _context.Payments.FirstOrDefault(p => p.ReservationId == reservation.ReservationId);
        if (payment != null)
        {
            payment.IsPaid = false;
        }

        _context.SaveChanges();

        TempData["Success"] = "Rezervasyon iptal edildi.";
        return RedirectToAction("Reservations");
    }
    public IActionResult Payments()
    {
        var payments = _context.Payments
            .Include(p => p.Reservation)
                .ThenInclude(r => r.Tenant) // Kiracıyı getir
            .Include(p => p.Reservation)
                .ThenInclude(r => r.House) // Evi getir
            .ToList();

        return View(payments);
    }

    public IActionResult HousesWithoutImages()
    {
        var result = _context.Database
    .SqlQueryRaw<int>("SELECT dbo.fn_CountHousesWithoutImage()")
    .AsEnumerable()
    .FirstOrDefault();

        ViewBag.MissingImageCount = result;

        var houses = _context.Houses
            .Where(h => string.IsNullOrEmpty(h.ImagePath))
            .ToList();

        return View(houses);
    }
    public IActionResult TopSpender()
    {
        var topUser = _context.TopSpenderViewModel
            .FromSqlRaw("SELECT * FROM fn_GetTopSpenderUser()")
            .AsEnumerable()
            .FirstOrDefault();

        return View(topUser);
    }

    public IActionResult ImageStats()
    {
        var result = _context.Database
            .SqlQueryRaw<int>("SELECT dbo.fn_CountHousesWithoutImage()")
            .AsEnumerable()
            .FirstOrDefault();

        ViewBag.MissingImageCount = result;
        return View();
    }





}
