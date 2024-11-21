using MultiplexCinema.Models;

public class ScreeningViewModel
{
    public Screening Screening { get; set; }
    public decimal OccupancyPercentage { get; set; }
    public int ChildrenCount { get; set; }
    public int AdultCount { get; set; }
    public string Status { get; set; }  // Dodanie Status do ViewModel
}