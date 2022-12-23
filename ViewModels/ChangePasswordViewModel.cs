using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.ViewModels
{
    public class ChangePasswordViewModel
    {
        // This current password is required to check is that owner of account is changing passord even thoug he has been signed in
        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Current Password")]
        public string CurrentPassword { get; set; }

        // The new password of the user
        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="New Password")]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Confirm New Password")]
        [Compare("NewPassword",ErrorMessage ="New Password and Confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}