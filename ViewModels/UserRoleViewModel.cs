namespace EmployeeManagementApp.ViewModels
{
    // We could include RoleId property also in the UserRoleViewModel class, but as far as this view is concerned, there is a one-to-many relationship from Role to Users. So, in order not to repeat RoleId for each User, we will use ViewBag to pass RoleId from controller to the view.

    public class UserRoleViewModel
    {
       public string UserId { get; set; }   

      // UserName property is required so we can display the UserName on the view. 
       public string UserName { get; set; }

       //    IsSelected property is required to determine if the user is selected to be a member of the role.
       public bool IsSelected { get; set; }
    }
}