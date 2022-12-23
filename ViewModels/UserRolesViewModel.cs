namespace EmployeeManagementApp.ViewModels
{
    // Here we need populate single role information of user like this list we need to send to Manageuserroles view
    public class UserRolesViewModel
    {
        public string RoleId { get; set; }

         // rolename property is required so we can display the rolename on the view. 
        public string RoleName { get; set; }

         //    IsSelected property is required to determine if the role is selected to be a assign to user.
        public bool IsSelected { get; set; }

    }
}