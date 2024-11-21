using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiplexCinema.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MultiplexCinema.Controllers
{
    public class AuditoriumsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditoriumsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Auditoriums
        public async Task<IActionResult> Index()
        {
            return View(await _context.Auditoriums.ToListAsync());
        }

        // GET: Auditoriums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var auditorium = await _context.Auditoriums
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditorium == null) return NotFound();

            return View(auditorium);
        }

        // GET: Auditoriums/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Auditoriums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SeatCount")] Auditorium auditorium)
        {
            if (ModelState.IsValid)
            {
                _context.Add(auditorium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(auditorium);
        }

        // GET: Auditoriums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var auditorium = await _context.Auditoriums.FindAsync(id);
            if (auditorium == null) return NotFound();

            return View(auditorium);
        }

        // POST: Auditoriums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SeatCount")] Auditorium auditorium)
        {
            if (id != auditorium.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auditorium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuditoriumExists(auditorium.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(auditorium);
        }

        // GET: Auditoriums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var auditorium = await _context.Auditoriums
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditorium == null) return NotFound();

            return View(auditorium);
        }

        // POST: Auditoriums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auditorium = await _context.Auditoriums.FindAsync(id);
            if (auditorium != null)
            {
                _context.Auditoriums.Remove(auditorium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuditoriumExists(int id)
        {
            return _context.Auditoriums.Any(e => e.Id == id);
        }
    }
}
