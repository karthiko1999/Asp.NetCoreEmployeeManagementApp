using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementApp.Security.Tokenlifespan
{
    // This is class used to provide lifespan specific to only for EmailConfirmation token, but if we use default DataProtectionTokenProviderOptions set lifespan it will set to all the tokens.
    public class CustomEmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        // here we are inherited all propertys such as LifeSpan of an Token property from base
    }
}