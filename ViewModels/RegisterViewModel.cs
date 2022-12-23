using System.ComponentModel.DataAnnotations;
using EmployeeManagementApp.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementApp.ViewModels
{
    public  class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        // This remote validation is also not working on mac
        // This way to tell the client side script to invoke server side method for this property get values. by using remote() we secifiy which method it has issue AJAX call.
        [Remote(action:"IsEmailInUse",controller:"Account")]
         // Here we are using custom attribute, here instaed hard codeing the domain allowed we use allowedDomain property in that class
        [ValidEmailDomain(allowedDoamin:"user.com",ErrorMessage ="Email with the domain @user.com is only allowed to register.")]
        public string Email {get;set;}

        [Required]
        [DataType(DataType.Password)]
        public string Password {get;set;}

        [Display(Name = "Confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // This is after the Custom IdentityUser Class is added to to Database to store city
         public string City { get; set; }
    }
}