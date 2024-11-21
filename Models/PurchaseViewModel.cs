using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class PurchaseViewModel
{
    [Required]
    public int? MovieId { get; set; }  // Identyfikator filmu

    [Required]
    public int? ScreeningId { get; set; } // Identyfikator seansu

    [Required]
    public string TicketType { get; set; } // Typ biletu (Normalny/Ulgowy)

    public decimal Price { get; set; } // Cena biletu

    public int? SeatNumber { get; set; } // Numer miejsca

    public List<SelectListItem> AvailableMovies { get; set; } = new List<SelectListItem>(); // Lista dostępnych filmów

    public List<SelectListItem> AvailableScreenings { get; set; } = new List<SelectListItem>(); // Lista dostępnych seansów
}