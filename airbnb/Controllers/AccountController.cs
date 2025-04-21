using airbnb.Models;
using airbnb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using airbnb.Helpers;

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
            // Şifreyi hashle
            string hashedPassword = airbnb.Helpers.PasswordHasher.Hash(password);

            // Hashlenmiş şifre ile kullanıcıyı kontrol et
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Hatalı e-posta veya şifre.";
                return View();
            }

            // Session başlat
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FirstName);
            HttpContext.Session.SetString("UserRole", user.Role.RoleName);

            // Rol bazlı yönlendirme
            if (user.Role.RoleName == "Admin")
                return RedirectToAction("Index", "Home");
            else if (user.Role.RoleName == "Ev Sahibi")
                return RedirectToAction("Index", "Home");
            else
                return RedirectToAction("Index", "Home");
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

            // Rol ID’sini al
            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == role);
            if (roleEntity == null)
            {
                ViewBag.Error = "Geçerli bir rol bulunamadı.";
                return View();
            }

            // Şifreyi hashle
            string hashedPassword = airbnb.Helpers.PasswordHasher.Hash(password);

            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = hashedPassword,
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
