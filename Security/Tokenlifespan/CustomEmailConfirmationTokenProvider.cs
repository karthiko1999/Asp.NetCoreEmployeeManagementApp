using EmployeeManagementApp.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EmployeeManagementApp.Security.Tokenlifespan
{
    // This is class which have the GenerateAsync() and ValidateAsync() methods to generate token, this calss is get used by the GenerateUserTokenAsync() where it inturn get called by UserManager.GeneratePaswordResetTokenAsync()  
    public class CustomEmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        // 1.IDataProtectionProvider is used to get insatnce IDataProtector type which will be have 2 methods Protect() and UnPrtotect().
        // 2.IOptions of DataProtectionTokenProviderOptions which is used to specifiy the custom lifespan of token
        // 3.Ilogger  which we expects same class,i.e an DataProtectorTokenProvider 
        public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<CustomEmailConfirmationTokenProviderOptions> options, ILogger<CustomEmailConfirmationTokenProvider<TUser>> logger) : base(dataProtectionProvider, options, (ILogger<DataProtectorTokenProvider<TUser>>)logger)
        {
        }
    }
}