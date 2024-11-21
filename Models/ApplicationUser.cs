using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MultiplexCinema.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [Display(Name = "Imię i nazwisko")]
    public string FullName { get; set; } = string.Empty;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}