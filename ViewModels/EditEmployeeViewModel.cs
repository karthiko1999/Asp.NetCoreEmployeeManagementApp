using System.ComponentModel.DataAnnotations;
using EmployeeManagementApp.Models;

namespace EmployeeManagementApp.ViewModels
{
    // Since : is used so eveery property in CreateEmployeeViewModel will be available in EditEmployeeViewModel ans some extra one are defined.
    public class EditEmployeeViewModel : CreateEmployeeViewModel
    {
        public string Id {get;set;}
        public string ExistingPhotoPath {get;set;}
        

    }
}