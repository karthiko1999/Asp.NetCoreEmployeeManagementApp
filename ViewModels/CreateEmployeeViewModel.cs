using System.ComponentModel.DataAnnotations;
using EmployeeManagementApp.Models;
using EmployeeManagementApp.Utilities;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagementApp.ViewModels
{
    public class CreateEmployeeViewModel
    {
        // ASP.NET Core tag helpers work in combination with the model validation attributes and generate the following HTML. Notice in the generated HTML we have data-* attributes. this data-val... attributes used to by server to do validation.
        // The data-* attributes allow us to add extra information to an HTML element. These data-* attributes carry all the information required to perform the client-side or server-side validation
        [Required(ErrorMessage ="Name of employee cannot be empty...")]
        [MaxLength(15,ErrorMessage ="Name cannot exceed the 15 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage ="Email cannot be empty...")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",ErrorMessage = "Invalid Email format")]
        public string Email { get; set; }

        [Required(ErrorMessage ="Please choose the valid options for Department..")]
        public Department? Department { get; set; }

        [Display(Name="EmployeePhoto")]
        public IFormFile Photo {get;set;}

    }
}

