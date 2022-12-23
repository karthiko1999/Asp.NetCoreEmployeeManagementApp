using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.ViewModels
{
    // TO be able to reset password we need user email,password,token
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string  Password { get; set; }

         [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string  ConfirmPassword { get; set; }
        public string Token { get; set; }
    }
}