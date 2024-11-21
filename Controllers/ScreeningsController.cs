using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiplexCinema.Models;
using System.Linq;
using System.Threading.Tasks;

public class ScreeningsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ScreeningsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Available for all users
    public async Task<IActionResult> Index()
    {
        var screenings = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Auditorium)
            .ToListAsync();

        var screeningViewModels = screenings.Select(s => new ScreeningViewModel
        {
            Screening = s,
            OccupancyPercentage = GetOccupancyPercentage(s),
            ChildrenCount = GetTicketCount(s, "Dziecięcy"),
            AdultCount = GetTicketCount(s, "Normalny"),
            Status = s.Status // Adding the status
        }).ToList();

        return View(screeningViewModels);
    }

    // Only for administrators - create screenings
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // Only for administrators - save new screening
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateScreening(Screening screening)
    {
        if (ModelState.IsValid)
        {
            _context.Add(screening);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(screening);
    }

    // Only for administrators - edit screening
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var screening = await _context.Screenings.FindAsync(id);
        if (screening == null)
        {
            return NotFound();
        }
        return View(screening);
    }

    // Only for administrators - save edited screening
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,ScreeningTime,AuditoriumId,Status")] Screening screening)
    {
        if (id != screening.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(screening);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScreeningExists(screening.Id))
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
        return View(screening);
    }

    // Only for administrators - delete screening
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var screening = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Auditorium)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (screening == null)
        {
            return NotFound();
        }

        return View(screening);
    }

    // Only for administrators - confirm deletion of screening
    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var screening = await _context.Screenings.FindAsync(id);
        if (screening != null)
        {
            _context.Screenings.Remove(screening);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // Check if the screening exists
    private bool ScreeningExists(int id)
    {
        return _context.Screenings.Any(e => e.Id == id);
    }

    // Calculate occupancy percentage for a given screening
    private decimal GetOccupancyPercentage(Screening screening)
    {
        var totalSeats = screening.Auditorium.SeatCount;
        var occupiedSeats = _context.Tickets
            .Where(t => t.ScreeningId == screening.Id)
            .Count();

        return totalSeats == 0 ? 0m : (decimal)occupiedSeats / totalSeats * 100;
    }

    // Count the number of tickets for a specific type (e.g., "Dziecięcy" or "Normalny")
    private int GetTicketCount(Screening screening, string ticketType)
    {
        return _context.Tickets
            .Where(t => t.ScreeningId == screening.Id && t.TicketType == ticketType)
            .Count();
    }
}
