using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiplexCinema.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        // Get screenings for today
        var screeningsToday = await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Auditorium)
            .Where(s => s.ScreeningTime.Date == today)
            .ToListAsync();

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var userTickets = await _context.Tickets
                .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
                .Include(t => t.Screening.Auditorium)
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            ViewBag.UserTickets = userTickets;
        }
        else
        {
            ViewBag.UserTickets = null;
        }

        if (User.IsInRole("Admin"))
        {
            var totalRevenue = await _context.Tickets
                .SumAsync(t => t.Price);

            ViewBag.TotalRevenue = totalRevenue;
        }

        return View(screeningsToday);
    }
}