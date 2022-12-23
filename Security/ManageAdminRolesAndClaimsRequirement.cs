using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagementApp.Security
{
    public class ManageAdminRolesAndClaimsRequirement : IAuthorizationRequirement
    {
        // To make class as Requirement that we adding to our policy make class derive from empty markup IAuthorizationRequirement interface
    }
}