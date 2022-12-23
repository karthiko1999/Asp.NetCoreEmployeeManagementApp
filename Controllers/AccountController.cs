using EmployeeManagementApp.Models;
using EmployeeManagementApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace EmployeeManagementApp.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private ILogger<AccountController> _logger;

        // Letus inject UserMangager contains the required methods to manage users in the underlying data store. For example, this class has methods like CreateAsync, DeleteAsync, UpdateAsync to create, delete and update users.
        // SigInManager class contains the required methods for users signin. For example, this class has methods like SignInAsync, SignOutAsync to signin and signout a user.
        // Both except a generic parameter to indicate on which specify the User class that these services should work with. 
        public AccountController(UserManager<ApplicationUser> user, SignInManager<ApplicationUser> sign, ILogger<AccountController> log)
        {
            _userManager = user;
            _signInManager = sign;
            _logger = log;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel registerUser)
        {
            if (ModelState.IsValid)
            {
                // create identity user instance
                // Copy data from RegisterViewModel to IdentityUser
                var user = new ApplicationUser()
                {
                    UserName = registerUser.Email,
                    Email = registerUser.Email,
                    City = registerUser.City
                };

                // Store user data in AspNetUsers database table
                var result = await _userManager.CreateAsync(user, registerUser.Password);

                // If user is successfully created, sign-in the user using SignInManager and redirect to index action/listofusers of HomeController
                if (result.Succeeded)
                {
                    // If user is entered there eamil we need to confirm,until he confirms email he was prompted as Email not confirmed login page
                    // Generater EmailConfirmation Token for user 
                    var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    // Once we have the email confirmation token we need to build confirmation link where user clicks on that link and confirm email
                    var emailConfirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = emailConfirmationToken }, Request.Scheme);
                    // Here we are logging the emailConfirmationLink to Console window using logger
                    _logger.LogWarning(emailConfirmationLink);


                    // If user is been created by user with Admin role redirect him to list user
                    if (_signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        // redirect to Listuser instead of login with new user
                        return RedirectToAction("ListUsers", "Administration");
                    }

                    // Send message to display registeration successFull but you need to Validate email by click link sent to your mail
                    ViewBag.Title = "Registration successful";
                    ViewBag.Message = "Before you can Login, please confirm your email, by clicking on the confirmation link we have emailed you.";
                    return View("ConfirmEmail");

                    // If new user is been cretaed by normal user or himself sign in and redirect him to index
                    // await _signInManager.SignInAsync(user, false);
                    // return RedirectToAction("index", "home");
                }

                // If there are any errors, add them to the ModelState object which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            return View("Register", registerUser);
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View("Login", model);
        }

        [HttpPost]
        [AllowAnonymous]
        // Through model binding we get the Query string parameter value RetirnUrl ,where the value of that paramete will be our ActuallUrl that we are trying to Acces before we Authenticate ,so it redirects to Account/Login/ReturnUrl?=value path.   
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl)
        {
            loginViewModel.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            loginViewModel.ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                // Here we need to check user is confirmed email or not before signin to our app,dispaly a error if he not conformed
                var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                // dispaly a error if he not conformed, we need to display only email not confirmed error if provide email and passsword are correct this to avoid brute force attack
                if (user != null && !user.EmailConfirmed && (await _userManager.CheckPasswordAsync(user, loginViewModel.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email is not Confirmed Yet.");
                    return View(loginViewModel);
                }




                // This method will do signin specified user and password combination,whether they need a Persistance cookie, session cookie.
                //  The last boolean parameter lockoutOnFailure indicates if the account should be locked on failed logon attempt. On every failed logon attempt AccessFailedCount column value in AspNetUsers table is incremented by 1.
                var sigInUser = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, true);


                if (sigInUser.Succeeded)
                {
                    // check if retirn url is null or value, and also check if its a local url or not.
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        // Then pass Actual url value available in returnUrl ,so user is redirected to that,after signin.
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        // If the returnUrl value is empty.
                        // After Successfull login user been redirected to Home
                        return RedirectToAction("index", "home");
                    }

                }

                // If account is been locked out the redirect user either to Reset password Or Wait till LockouEnd Time span will over.
                if(sigInUser.IsLockedOut)
                {
                    return View("AccountLocked");
                }

                ModelState.AddModelError("", "Invalid Login Attempt.Try again...");
            }
            return View(loginViewModel);
        }


        // This remote validation Method, where we are client side script to call this method,this is useful when we need to call server side method without a full page postback
        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            // In order check whetere the email is already in use or not use the Usermanger service and call FindByEmailAsync() 
            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            // Check user null then email not in use.
            if (user == null)
            {
                // Since the AJAX call is been issued to this IsEmailInUse() by remote() jquery so it expects a json back.
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already in use - Remote Validation");
            }
        }

        // Configure redirect request from Google after user click on SignInWith google or any providers
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            // after the successfull authentication of user in google has to redirect the Application controller
            var redirectUrl = Url.Action("ExternalLoginCallBack", "Account", new { ReturnUrl = returnUrl });

            // Setting/Configure some  Properties for external authentication providers
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // call provider
            return new ChallengeResult(provider, properties);
        }

        // Handle request or identity received from the external identity providers
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins =
                        (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return View("Login", loginViewModel);
            }

            // Get the login information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }

            // Get the email claim from external login provider (Google, Facebook etc), this below are changed for email confirmation
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = null;

            if (email != null)
            {
                // Find the user
                user = await _userManager.FindByEmailAsync(email);

                // If email is not confirmed, display login view with validation error
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet. " + info.LoginProvider);
                    return View("Login", loginViewModel);
                }
            }

            // If the user already has a login (i.e if there is a record in AspNetUserLogins
            // table) then sign-in the user with this external login provider
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            // If there is no record in AspNetUserLogins table, the user may not have
            // a local account
            else
            {
                // Get the email claim value
                // var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    // Create a new user without password if we do not have a user already
                    // user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        // Here we are creating the new user
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await _userManager.CreateAsync(user);

                        // After creating user we need to generate token and emailconfimation link and log that to Console 
                        var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var emailConfirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = emailConfirmationToken }, Request.Scheme);
                        _logger.LogWarning(emailConfirmationLink);
                        ViewBag.Title = "Registration successful";
                        ViewBag.Message = "Before you can Login, please confirm your email, by clicking on the confirmation link we have emailed you. ";
                        return View("ConfirmEmail");
                    }

                    // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                // If we cannot find the user email we cannot continue
                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support on micro@microtech.com";

                return View("Views/Error/Error.cshtml");
            }
        }

        // After user cliks or issues a EmailConfirmation button or link that is sent to his mail or Logged to console
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            // We need to check the userId and token values are successFully received or not
            if (userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }
            // Check user with id exist
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.Message = $"The User ID {userId} is invalid";
                return View("~/Views/Error/NotFound.cshtml");
            }

            // we need confirm email of user by using token that we generated for confirmation email
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ViewBag.Title = "Email confirmed Successfully...!";
                ViewBag.SomeInformation = "Now you can login using your Credentials,Click on Login Button on Top rigth.";
                return View("ConfirmEmail");
            }

            ViewBag.ErrorTitle = "Email cannot be confirmed";
            ViewBag.ErrorMessage = "Please click on valid verfification link";
            return View("~/Views/Error/Error.cshtml");
        }

        
        [HttpGet]
        // This method that for dispaly view for to take eamilid to generate reset password link
        [AllowAnonymous]
        public IActionResult ForgetPassword()
        {
            return View();
        }
        // We get email address from user for which we need generate password reset link
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel forgetPasswordViewModel)
        {
            // we need to check model is valid
            if (ModelState.IsValid)
            {
                // Find user by email id provide by form
                ApplicationUser applicationUser = await _userManager.FindByEmailAsync(forgetPasswordViewModel.Email);

                // If we find user check is he confirmed email this becuase we send reset link to his mail
                if (applicationUser != null && await _userManager.IsEmailConfirmedAsync(applicationUser))
                {
                    // generate password reset token for user using  GeneratePasswordResetTokenAsync of usermanager
                    var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
                    // Build a complete  password reset link
                    var passwordResetLink = Url.Action("ResetPassword", "Account", new { email = forgetPasswordViewModel.Email, token = passwordResetToken }, Request.Scheme);
                    // Log to console window
                    _logger.LogWarning(passwordResetLink);
                    // Return user to view 
                    return View("ForgotPasswordConfirmation");
                }
                // If we cannot able to find user with email we also return same view,because we don't want to reveal user doesnot exists
                // To avoid account enumeration and brute force attacks, don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");

            }
            // If there is any validation errors we display
            return View(forgetPasswordViewModel);
        }


        // After user clicks on reset password link that we have logged he as redirected to this with email and token in query
        // Here we need to return View to ask for new password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            // Check the email and token received from reset password link
            if (email == null || token == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid password reset token");
            }
            return View();
        }
        // Post method with ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                // Find user with email to reset his password 
                var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);

                // Check user is found 
                if (user != null)
                {
                    // This is to update user with new password by using ResetPasswordAsync() of usermanager by provide password reset token
                    var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token, resetPasswordViewModel.Password);

                    // if successfully reset password
                    if (result.Succeeded)
                    {
                        // before password reset confirmation view if used rested his password after his account lockout from AccountLocked view.
                        // we need set lockoutend column to certain current value,so that user can login using reseted new password.
                        if(await _userManager.IsLockedOutAsync(user))
                        {
                           await _userManager.SetLockoutEndDateAsync(user,DateTimeOffset.UtcNow);
                        }

                        return View("ResetPasswordConfirmation");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                    // This is to disply errors
                    return View(resetPasswordViewModel);
                }
                // Return same reset password confirmation view To avoid account enumeration and brute force attacks, don't reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }
            // dispaly if any validation is there
            return View(resetPasswordViewModel);
        }

        // It methods that responsible change  current password of signed user
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            // here we need siply return ChangePassword,with that we are user current password ,newpassword ,confirmnewpassword
            // since user is already signed eventhough we are asking him to provide is current password this because to avoi a maliciou user from changing user password if he forgort logout from pubic computer or he has briefly walked away from the computer 
            // Here we also check is user using external login account then they need add password first after he can change that , if Password Hash column is null redirect user to AddPassword()

            var user = await _userManager.GetUserAsync(User);
            if (user.PasswordHash == null)
            {
                // he need to add password first in order to change
                return RedirectToAction("AddPassword");
            }
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                // retrive existing signed user using ControllerBase.User claim return ClaimsPrincipal
                // GetUserAsync() - Returns the user corresponding to the IdentityOptions.ClaimsIdentity.UserIdClaimType claim in the principal or null.
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    // something wrong has been happen so reditect user to login view
                    return RedirectToAction("Login");
                }

                // if find user change his password
                var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword);

                //  The new password did not meet the complexity rules or the current password is incorrect. Add these errors to the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // we need call RefreshSignAsync() in order to refresh signIn cookie of logned in user
                // Upon successfully changing the password refresh sign-in cookie
                await _signInManager.RefreshSignInAsync(user);
                // If password changed successfully,we return Password Changed SuccessFully.
                return View("ChangePasswordConfirmation");

            }
            return View(changePasswordViewModel);
        }

        // User can add the password to local account that linked external account because when user using external account to login password column of User account is null in AspNetUsers table. who used there extenal login providers.
        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            // check user who is trying to add password having password column null,this means he doesnot setted his password previously for account linked external account.
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                // something wrong has been happen so reditect user to login view
                return RedirectToAction("Login");
            }

            // check password column
            if (user.PasswordHash != null)
            {
                // user already setted his password redirect him to change password action
                return RedirectToAction("ChangePassword");
            }
            // else show add password view
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel addPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                // Get Signed user who want to add a password for there local account linked with external account
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // something wrong has been happen so reditect user to login view
                    return RedirectToAction("Login");
                }

                // If we find user the add password by using AddPasswordAsync() of Usermanager 
                var result = await _userManager.AddPasswordAsync(user, addPasswordViewModel.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(addPasswordViewModel);
                }
                // else refersh sign cookie of user after password is been added
                await _signInManager.RefreshSignInAsync(user);
                // after adding and  refersh signin cookie of user return confirmation view.
                return View("AddPasswordConfirmation");

            }
            return View(addPasswordViewModel);
        }

    }
}
