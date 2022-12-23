using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.ViewModels
{
    // This is class that used to collect email address of user for whom to generate password reset link 
    public class ForgetPasswordViewModel
    {
        // Email address is only field required
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}