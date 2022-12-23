using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name ="Role")]
        public string RoleName {get;set;}
    }
}