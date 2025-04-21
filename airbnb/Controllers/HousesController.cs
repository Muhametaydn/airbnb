using System;
using System.Collections.Generic;
using System.Linq;
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
            {
                throw new Exception("Kullanıcı oturum açmamış yada yetkisiz giris.");
            }
            return int.Parse(userIdClaim.Value);
        }


        // GET: Houses
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var applicationDbContext = _context.Houses.Include(h => h.Owner);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Houses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var house = await _context.Houses
                .Include(h => h.Owner)
                .FirstOrDefaultAsync(m => m.HouseId == id);
            if (house == null)
            {
                return NotFound();
            }

            return View(house);
        }

        // GET: Houses/Create
        public IActionResult Create()
        {
            Console.WriteLine("create get house girdi----------");
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Host")
                return RedirectToAction("AccessDenied", "Account");

            ViewData["OwnerId"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: Houses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(House house)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Host")
                return RedirectToAction("AccessDenied", "Account");

            try
            {
                Console.WriteLine("VS DEBUG WORKING DIR: " + Directory.GetCurrentDirectory());

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
                    Directory.CreateDirectory(uploadsFolder); // klasör yoksa oluştur

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    try
                    {
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await house.ImageFile.CopyToAsync(stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("🔥 FOTOĞRAF KAYIT HATASI");
                        Console.WriteLine("📄 Message: " + ex.Message);
                        Console.WriteLine("📄 StackTrace: " + ex.StackTrace);
                        ModelState.AddModelError("ImageFile", "Fotoğraf kaydedilirken bir hata oluştu.");
                        return View(house);
                    }

                    house.ImagePath = "/images/" + fileName;
                }
                else
                {
                    Console.WriteLine("ImageFile NULL veya boş geldi.");
                }

                house.OwnerId = userId.Value;
                house.CreatedAt = DateTime.Now;
                house.IsActive = true;

                _context.Add(house);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("🚨 GENEL HATA: " + ex.Message);
                Console.WriteLine("📃 StackTrace: " + ex.StackTrace);
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
                return View(house);
            }
        }







        // GET: Houses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var house = await _context.Houses.FindAsync(id);
            if (house == null)
                return NotFound();

            var currentUserId = GetCurrentUserId(); // aşağıda yazacağız

            if (house.OwnerId != currentUserId)
                return Forbid(); // yetki yok hatası

            return View(house);
        }


        // POST: Houses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(House house)
        {
            var original = await _context.Houses.AsNoTracking().FirstOrDefaultAsync(h => h.HouseId == house.HouseId);
            if (original == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (original.OwnerId != currentUserId)
                return Forbid();

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
            {
                return NotFound();
            }

            var house = await _context.Houses
                .Include(h => h.Owner)
                .FirstOrDefaultAsync(m => m.HouseId == id);
            if (house == null)
            {
                return NotFound();
            }

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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HouseExists(int id)
        {
            return _context.Houses.Any(e => e.HouseId == id);
        }


    }

}
