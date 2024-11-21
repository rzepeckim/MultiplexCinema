using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Zapamiętaj mnie")]
    public bool RememberMe { get; set; }
}