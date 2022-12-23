using System.Security.Claims;

namespace EmployeeManagementApp.ViewModels
{
    // To keep it simple for our application we are storing the claims in the static class
    public static class ClaimsStore
    {
        // here we have a list of claims
        public static List<Claim> AllClaims = new List<Claim>()
        {
            // claims are name value pair which can be used for making access control decisions.where as it excepts the claimType,claimValue
            new Claim("Create Role","Create Role"),
            new Claim("Edit Role","Edit Role"),
            new Claim("Delete Role","Delete Role")
        };
    }
}