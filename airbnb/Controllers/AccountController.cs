using airbnb.Models;
using airbnb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace airbnb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Hatalı e-posta veya şifre.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FirstName);
            HttpContext.Session.SetString("UserRole", user.Role.RoleName);

            // Role'a göre yönlendirme
            if (user.Role.RoleName == "Admin")
                return RedirectToAction("Index", "Users");
            else if (user.Role.RoleName == "Ev Sahibi")
                return RedirectToAction("Index", "Houses");
            else
                return RedirectToAction("Index", "Reservations");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string password, string role = "Tenant")
        {
            // Aynı mailden var mı kontrolü
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Bu e-posta zaten kayıtlı.";
                return View();
            }

            // Rol ID’sini al (eğer veritabanında hazır roller varsa)
            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == role);
            if (roleEntity == null)
            {
                ViewBag.Error = "Geçerli bir rol bulunamadı.";
                return View();
            }

            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = password,
                RoleId = roleEntity.RoleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
