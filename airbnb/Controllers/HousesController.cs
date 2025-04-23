using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using airbnb.Data;
using airbnb.Models;

namespace airbnb.Controllers
{
    public class HousesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HousesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                throw new Exception("Kullanıcı oturum açmamış yada yetkisiz giriş.");
            return int.Parse(userIdClaim.Value);
        }

        // GET: Houses
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var currentUserId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<House> houses = _context.Houses.Include(h => h.Owner);

            if (userRole != "Admin")
            {
                houses = houses.Where(h => h.OwnerId == currentUserId);
            }

            return View(await houses.ToListAsync());
        }




        // GET: Houses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var house = await _context.Houses
                .Include(h => h.Owner)
                .FirstOrDefaultAsync(h => h.HouseId == id);

            if (house == null)
                return NotFound();

            // Sadece aktif rezervasyonlar gelsin (iptal edilenler hariç)
            var reservations = await _context.Reservations
                .Where(r => r.HouseId == id && r.Status != "Cancelled")
                .ToListAsync();

            ViewData["Reservations"] = reservations;

            return View(house);
        }




        // GET: Houses/Create
        public IActionResult Create()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            if (User.FindFirst(ClaimTypes.Role)?.Value != "Host")
                return RedirectToAction("AccessDenied", "Account");

            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: Houses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(House house)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            if (User.FindFirst(ClaimTypes.Role)?.Value != "Host")
                return RedirectToAction("AccessDenied", "Account");

            var userId = GetCurrentUserId();

            try
            {
                if (house.ImageFile != null && house.ImageFile.Length > 0)
                {
                    var extension = Path.GetExtension(house.ImageFile.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Sadece .jpg, .jpeg, .png veya .gif dosyalarına izin verilir.");
                        return View(house);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await house.ImageFile.CopyToAsync(stream);

                    house.ImagePath = "/images/" + fileName;
                }

                house.OwnerId = userId;
                house.CreatedAt = DateTime.Now;
                house.IsActive = true;

                _context.Add(house);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("🚨 HATA: " + ex.Message);
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu.");
                return View(house);
            }
        }

        // GET: Houses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var house = await _context.Houses.FindAsync(id);
            if (house == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (house.OwnerId != currentUserId)
                return Forbid();

            return View(house);
        }

        // POST: Houses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(House house)
        {
            var original = await _context.Houses.AsNoTracking().FirstOrDefaultAsync(h => h.HouseId == house.HouseId);
            if (original == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (original.OwnerId != currentUserId)
                return Forbid();

            // 🧠 FOTOĞRAF DEĞİŞTİRME KONTROLÜ
            if (house.ImageFile != null && house.ImageFile.Length > 0)
            {
                var extension = Path.GetExtension(house.ImageFile.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Sadece .jpg, .jpeg, .png veya .gif dosyalarına izin verilir.");
                    return View(house);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await house.ImageFile.CopyToAsync(stream);

                house.ImagePath = "/images/" + fileName;
            }
            else
            {
                // Fotoğraf seçilmediyse eski fotoğrafı koru
                house.ImagePath = original.ImagePath;
            }

            // Sabit kalanlar
            house.OwnerId = original.OwnerId;
            house.CreatedAt = original.CreatedAt;

            _context.Update(house);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Houses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var house = await _context.Houses
                .Include(h => h.Owner)
                .FirstOrDefaultAsync(m => m.HouseId == id);

            if (house == null)
                return NotFound();

            return View(house);
        }

        // POST: Houses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var house = await _context.Houses.FindAsync(id);
            if (house != null)
            {
                _context.Houses.Remove(house);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HouseExists(int id)
        {
            return _context.Houses.Any(e => e.HouseId == id);
        }
    }
}
