using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeManagementApp.Security
{
    // Logic either to provide access or not is in handler
    // makes this handler derive from IAuthorizationHandler <T>,where T is requiremnt of handler
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimsRequirement manageAdminRolesAndClaimsRequirement)
        {
            // We get resource we protecting using AuthorizationHandlerContext ,i.e is controller action mmethod 
            // Resource as AuthorizationFilterContext provide access to Httpcontext,RouteData.etc everthing provided by MVC and RazorPages
            var authorizationFilterContext = context.Resource as AuthorizationFilterContext;
            if(authorizationFilterContext == null)
            {
                // we return access not authorized
                return Task.CompletedTask;
            }

            // here we need to check requirements met or not
            
            // First we need get loggedin AdimId
            var loggedInAdminId = context.User.Claims.FirstOrDefault(claim=>claim.Type == ClaimTypes.NameIdentifier).Value;
            // Secong we need EditingAdminId in our application it is passes as Routedata instead of Querystring
            string adminIdBeingEdited = authorizationFilterContext.HttpContext.Request.Query["userId"];
            
            // we check loggedInAdminId, adminBeingEdited have same id then access has denied otherwise allow access
             if (context.User.IsInRole("Admin") &&
            context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") &&
            adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
            {
                // provide access by indication requirement is Succeded for AdminUser
                context.Succeed(manageAdminRolesAndClaimsRequirement);
            }
            // else{
            //     context.Fail();
            // } Avoid of returing failure from handler because eventohugh others handler of same requirement returning succeed policy will going to fail.

            // then we have to return task is completed
            return Task.CompletedTask;
        }
    }
}