using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaHub.MVC.Models;

public class AppUser : IdentityUser
{
    [Required]
    [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters and must be at least 2 characters long.", MinimumLength = 2)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters and must be at least 2 characters long.", MinimumLength = 2)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";
}
