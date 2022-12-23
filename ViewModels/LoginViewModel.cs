using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace EmployeeManagementApp.ViewModels
{
    public class LoginViewModel
    {
       public LoginViewModel()
       {
         ExternalLogins = new List<AuthenticationScheme>();
       }
       
        [Required(ErrorMessage ="Email field cannot be empty")]
        [EmailAddress(ErrorMessage ="Email is not in correct format")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

         [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        // this is below for external identity provider puprose
        // Return basically what user has returned after successfull authentication
        public string ReturnUrl { get; set; }

        // Store list of external logins that are enabled with our application
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

    }
}