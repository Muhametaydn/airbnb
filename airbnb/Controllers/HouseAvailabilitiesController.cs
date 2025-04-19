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
    public class HouseAvailabilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HouseAvailabilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HouseAvailabilities
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var applicationDbContext = _context.HouseAvailabilities.Include(h => h.House);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: HouseAvailabilities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var houseAvailability = await _context.HouseAvailabilities
                .Include(h => h.House)
                .FirstOrDefaultAsync(m => m.AvailabilityId == id);
            if (houseAvailability == null)
            {
                return NotFound();
            }

            return View(houseAvailability);
        }

        // GET: HouseAvailabilities/Create
        public IActionResult Create()
        {
            ViewData["HouseId"] = new SelectList(_context.Houses, "HouseId", "Description");
            return View();
        }

        // POST: HouseAvailabilities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AvailabilityId,HouseId,AvailableFrom,AvailableTo")] HouseAvailability houseAvailability)
        {
            if (ModelState.IsValid)
            {
                _context.Add(houseAvailability);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HouseId"] = new SelectList(_context.Houses, "HouseId", "Description", houseAvailability.HouseId);
            return View(houseAvailability);
        }

        // GET: HouseAvailabilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var houseAvailability = await _context.HouseAvailabilities.FindAsync(id);
            if (houseAvailability == null)
            {
                return NotFound();
            }
            ViewData["HouseId"] = new SelectList(_context.Houses, "HouseId", "Description", houseAvailability.HouseId);
            return View(houseAvailability);
        }

        // POST: HouseAvailabilities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AvailabilityId,HouseId,AvailableFrom,AvailableTo")] HouseAvailability houseAvailability)
        {
            if (id != houseAvailability.AvailabilityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(houseAvailability);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HouseAvailabilityExists(houseAvailability.AvailabilityId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HouseId"] = new SelectList(_context.Houses, "HouseId", "Description", houseAvailability.HouseId);
            return View(houseAvailability);
        }

        // GET: HouseAvailabilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var houseAvailability = await _context.HouseAvailabilities
                .Include(h => h.House)
                .FirstOrDefaultAsync(m => m.AvailabilityId == id);
            if (houseAvailability == null)
            {
                return NotFound();
            }

            return View(houseAvailability);
        }

        // POST: HouseAvailabilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var houseAvailability = await _context.HouseAvailabilities.FindAsync(id);
            if (houseAvailability != null)
            {
                _context.HouseAvailabilities.Remove(houseAvailability);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HouseAvailabilityExists(int id)
        {
            return _context.HouseAvailabilities.Any(e => e.AvailabilityId == id);
        }
    }
}
