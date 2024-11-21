using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Imię i nazwisko")]
    public string FullName { get; set; } = string.Empty; // Initialize with default value

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty; // Initialize with default value

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty; // Initialize with default value

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Potwierdź hasło")]
    public string ConfirmPassword { get; set; } = string.Empty; // Initialize with default value
}