using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiplexCinema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Tickets/Purchase
    [HttpGet]
    public async Task<IActionResult> Purchase()
    {
        var model = await GetPurchaseViewModelAsync();
        return View(model);
    }

    // AJAX: Pobierz dostępne seanse dla wybranego filmu
    [HttpGet]
    public async Task<IActionResult> GetScreeningsForMovie(int movieId)
    {
        var screenings = await _context.Screenings
            .Where(s => s.MovieId == movieId)
            .OrderBy(s => s.ScreeningTime)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.ScreeningTime:dd MMM yyyy HH:mm} - Sala: {s.Auditorium.Name}"
            })
            .ToListAsync();

        return Json(screenings);
    }

    // POST: Tickets/Purchase (potwierdzenie zakupu biletu)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purchase(PurchaseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableMovies = await GetMoviesSelectListAsync();
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var screening = await _context.Screenings
            .Include(s => s.Auditorium)
            .FirstOrDefaultAsync(s => s.Id == model.ScreeningId);

        if (screening == null)
        {
            ModelState.AddModelError("", "Wybrany seans nie istnieje.");
            return View(model);
        }

        // Check for taken seats
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

        // Create the ticket
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

    // Function to find an available seat
    private int? GetAvailableSeat(Auditorium auditorium, List<int> takenSeats)
    {
        for (int i = 1; i <= auditorium.SeatCount; i++)
        {
            if (!takenSeats.Contains(i))
            {
                return i;
            }
        }
        return null;
    }

    // GET: Tickets/MyTickets (User's ticket list)
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

    // Helper method to fetch movies select list
    private async Task<List<SelectListItem>> GetMoviesSelectListAsync()
    {
        var movies = await _context.Movies.ToListAsync();
        return movies.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(),
            Text = m.Title
        }).ToList();
    }

    // Helper method to get the Purchase view model with movies
    private async Task<PurchaseViewModel> GetPurchaseViewModelAsync()
    {
        var model = new PurchaseViewModel
        {
            AvailableMovies = await GetMoviesSelectListAsync()
        };
        return model;
    }
}
