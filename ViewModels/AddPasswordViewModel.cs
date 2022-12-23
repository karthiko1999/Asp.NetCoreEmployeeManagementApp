using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.ViewModels
{
    public  class AddPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword",ErrorMessage = "New Password and Confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}