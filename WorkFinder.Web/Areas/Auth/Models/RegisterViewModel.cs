using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WorkFinder.Web.Areas.Auth.Models;

public class RegisterViewModel
{
    [Required]
    [DisplayName("Register as")]
    public string Role { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DisplayName("Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required]
    [DisplayName("Terms of Service")]
    public bool AgreeTerms { get; set; }
}