using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiplexCinema.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class PurchaseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PurchaseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Purchase
    public IActionResult Purchase()
    {
        var model = new PurchaseViewModel
        {
            AvailableMovies = _context.Movies
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Title
                })
                .ToList()
        };

        return View(model);
    }

    // AJAX: Get available screenings for selected movie
    [HttpGet]
    public IActionResult GetScreeningsForMovie(int movieId)
    {
        var screenings = _context.Screenings
            .Where(s => s.MovieId == movieId)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.ScreeningTime:dd/MM/yyyy HH:mm} - Sala: {s.Auditorium.Name}"
            })
            .ToList();

        return Json(screenings);
    }

    // POST: Purchase
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purchase(PurchaseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Uzupełnij wszystkie wymagane pola.");
            PopulateMoviesAndScreenings(model);
            return View(model);
        }

        var screening = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Auditorium)
            .FirstOrDefaultAsync(s => s.Id == model.ScreeningId);

        if (screening == null)
        {
            ModelState.AddModelError("", "Wybrany seans nie istnieje.");
            PopulateMoviesAndScreenings(model);
            return View(model);
        }

        // Check taken seats for the selected screening
        var takenSeats = await _context.Tickets
            .Where(t => t.ScreeningId == model.ScreeningId)
            .Select(t => t.SeatNumber)
            .ToListAsync();

        var availableSeat = GetAvailableSeat(screening.Auditorium, takenSeats);
        if (availableSeat == null)
        {
            ModelState.AddModelError("", "Brak dostępnych miejsc na wybrany seans.");
            return View(model);
        }

        // Set seat and calculate price
        model.SeatNumber = availableSeat.Value;
        model.Price = model.TicketType == "Normalny" ? 20m : 15m;

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            ModelState.AddModelError("", "Użytkownik musi być zalogowany, aby kupić bilet.");
            PopulateMoviesAndScreenings(model);
            return View(model);
        }

        // Create ticket
        var ticket = new Ticket
        {
            ScreeningId = model.ScreeningId.Value,
            SeatNumber = model.SeatNumber.Value,
            Price = model.Price,
            UserId = user.Id,
            TicketType = model.TicketType
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Zakup biletu zakończony sukcesem!";
        return RedirectToAction("MyTickets");
    }

    // Helper function to find available seat
    private int? GetAvailableSeat(Auditorium auditorium, List<int> takenSeats)
    {
        // Find the first available seat that is not taken
        return Enumerable.Range(1, auditorium.SeatCount)
                         .FirstOrDefault(seat => !takenSeats.Contains(seat));
    }

    // Helper method to reload movies and screenings for the model
    private void PopulateMoviesAndScreenings(PurchaseViewModel model)
    {
        model.AvailableMovies = _context.Movies
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Title
            })
            .ToList();

        if (model.MovieId.HasValue)
        {
            model.AvailableScreenings = _context.Screenings
                .Include(s => s.Auditorium)
                .Where(s => s.MovieId == model.MovieId)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.ScreeningTime:dd/MM/yyyy HH:mm} - Sala: {s.Auditorium.Name}"
                })
                .ToList();
        }
        else
        {
            model.AvailableScreenings = new List<SelectListItem>(); // Empty if no movie selected
        }
    }

    // GET: Tickets/MyTickets (User's tickets list)
    [HttpGet]
    public async Task<IActionResult> MyTickets()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var tickets = await _context.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
            .Include(t => t.Screening.Auditorium)
            .Where(t => t.UserId == user.Id)
            .ToListAsync();

        return View(tickets);
    }
}
