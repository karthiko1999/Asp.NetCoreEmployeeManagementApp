using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementApp.Models
{
    // In oredr use Extend the Identity User of application,make class Dervie from IdentityUser class provide Asp.net core Identity Framework and replace all its reference 
    // This becuse base IdentityUser class will limited property such name,email,...,but if we want extra field like City,Country,Gender the we need to extend
    public class ApplicationUser : IdentityUser
    {
        public string City { get; set; }
    }
}