using System.ComponentModel.DataAnnotations;
using EmployeeManagementApp.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementApp.ViewModels
{

  public class EditUserViewModel
  {
    // In order to avoid null reference exception,intialize claims and Rolse
    public EditUserViewModel()
    {
        Roles = new List<string>();
        Claims = new List<string>();
    }

    [Display(Name ="User Id")]
    public string id { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    [ValidEmailDomain(allowedDoamin:"micro.com",ErrorMessage ="Email with the domain @micro.com is only allowed to register.")]
    public string Email { get; set; }

    public string City { get; set; }

    public List<string> Roles { get; set; }
    public List<string> Claims { get; set; }
  }

}