using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.ViewModels
{
    public class EditRoleViewModel
    {
        public EditRoleViewModel()
        {
            Users = new List<string>();
        }
        [Display(Name ="Role Id")]
        public string id { get; set; }

        [Required(ErrorMessage ="Role Name is Required")]
        public string RoleName { get; set; }

        // In order to avoid null reference exception use ctor() to intialize it.
        public List<string> Users { get; set; } 
    }
}