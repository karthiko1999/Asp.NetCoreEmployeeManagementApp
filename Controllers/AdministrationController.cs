using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeManagementApp.Models;
using EmployeeManagementApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementApp.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ILogger<AdministrationController> _logger;


        // Inject an Instance of RoleManager<T> where T:class where class = IdentityRole 
        // This role manager class will contain all methos required to perofrom Crud opertaion on Aspnetroles tables.
        // Inject the usermanager services which contains method to perform crud operations on IdentityUser 
        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<AdministrationController> Log)
        {
            this._roleManager = roleManager;
            this._userManager = userManager;
            this._logger = Log;
        }



        /* ---------------------------------------------------- Manage IdentityRole ----------------------------------------------------*/

        [HttpGet]
        public IActionResult ListRoles()
        {
            // In order to retieve all roles in AspNetRoles table user roleManager.Roles property and pass this data to view using Stronglytyped way.
            var roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        [Authorize(Policy="CreateRolePolicy")]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy="CreateRolePolicy")]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel createRoleViewModel)
        {
            if (ModelState.IsValid)
            {
                // Create an instance of IdentityRole from view model data to update role in AspnetRoles database table.
                var identityRole = new IdentityRole()
                {
                    Name = createRoleViewModel.RoleName
                };

                // Use RoleManager service to add role to Database table
                var identityResult = await _roleManager.CreateAsync(identityRole);

                // check if role created successfully or not
                if (identityResult.Succeeded)
                {
                    return RedirectToAction("index", "home");
                }
                // If there is any error in creating a new role we need show error on view

                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }
            return View(createRoleViewModel);
        }

        // When user click on Edit on List Roles we issue httpget to EditRole() 
        [HttpGet]
         [Authorize(Policy="Edit RolePolicy")]
        public async Task<IActionResult> EditRole(string id)
        {
            // Check role with id exists in Database or not using rolemanager
            IdentityRole identityRole = await _roleManager.FindByIdAsync(id);

            //If role doesnot exists we issue NotFound view instead of default 
            if (identityRole == null)
            {
                // here we want return notfound view
                ViewBag.Message = $"The Role with Id = {id} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }
            // create an instance of EditRoleViewModel and pass that EditRole View
            EditRoleViewModel editRoleViewModel = new EditRoleViewModel()
            {
                id = identityRole.Id,
                RoleName = identityRole.Name
            };

            // retirve all user and check wheteher is there any user the role  by calling IsInRoleAsync(user ,rolename) of rolemanager services 
            foreach (var user in _userManager.Users.ToList())
            {
                if (await _userManager.IsInRoleAsync(user, identityRole.Name))
                {
                    // If user is in the role then add user name to list of EditRoleviewmodel
                    editRoleViewModel.Users.Add(user.Email);
                }
            }

            return View(editRoleViewModel);
        }


        // When user edit and submit the form 
        [HttpPost]
        [Authorize(Policy="Edit RolePolicy")]
        public async Task<IActionResult> EditRole(EditRoleViewModel editRoleViewModel)
        {
            // Check role with id exists in Database or not using rolemanager
            IdentityRole identityRole = await _roleManager.FindByIdAsync(editRoleViewModel.id);

            //If role doesnot exists we issue NotFound view instead of default 
            if (identityRole == null)
            {
                // here we want return notfound view
                ViewBag.Message = $"The Role with Id = {editRoleViewModel.id} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }

            //override the values of alreday existing with this new one
            identityRole.Name = editRoleViewModel.RoleName;


            // Update database with role edited. using updateAsync() of rolemanager
            var identityResult = await _roleManager.UpdateAsync(identityRole);

            if (identityResult.Succeeded)
            {
                // Update is successfull return to ListRoles
                return RedirectToAction("ListRoles");
            }
            // else if any error show those
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(editRoleViewModel);
        }

        [HttpGet]
        
        // When user clicks on edit users in role button on editrole view we will routed here
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            // Check role with id exists or not
            var identityRole = await _roleManager.FindByIdAsync(roleId);

            if (identityRole == null)
            {
                // here we want return notfound view
                ViewBag.Message = $"The Role with Id = {roleId} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }

            // lets get all users from table.
            var identityUsers = _userManager.Users.ToList();

            // This the data send to view
            var _userRoleViewModels = new List<UserRoleViewModel>();

            foreach (var identityUser in identityUsers)
            {
                var userRoleViewModel = new UserRoleViewModel()
                {
                    UserName = identityUser.Email,
                    UserId = identityUser.Id
                };

                // check the user in role or not
                if (await _userManager.IsInRoleAsync(identityUser, identityRole.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                _userRoleViewModels.Add(userRoleViewModel);
            }

            // To send the roleId data to view using viewbag,so inorder to not repeat we use this
            ViewBag.roleId = roleId;
            // Additionally uses the name
            ViewBag.roleName = identityRole.Name;
            return View(_userRoleViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> userRoleViewModels, string roleId)
        {
            // Check role with id exists or not
            var identityRole = await _roleManager.FindByIdAsync(roleId);

            if (identityRole == null)
            {
                // here we want return notfound view
                ViewBag.Message = $"The Role with Id = {roleId} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }


            // Lets add roleId and user Id to AspNetUserRoles table
            for (int i = 0; i < userRoleViewModels.Count; i++)
            {
                // check the user exists or not
                var identityUser = await _userManager.FindByIdAsync(userRoleViewModels[i].UserId);
                if (identityUser == null)
                {
                    // here we want return notfound view
                    ViewBag.Message = $"The User with Id = {userRoleViewModels[i].UserId} Cannot Be Found";
                    return View("~/Views/Error/NotFound.cshtml");
                }


                IdentityResult identityResult = null;

                // check the user with role is already exists ,3 cases
                // 1.user without role previously and current, 2.user with role is now selected previously not have, 3.user with role is now deselected prevesiouly selected, 
                // user for role is now selected previously not have(2)
                if (userRoleViewModels[i].IsSelected && !(await _userManager.IsInRoleAsync(identityUser, identityRole.Name)))
                {
                    identityResult = await _userManager.AddToRoleAsync(identityUser, identityRole.Name);
                }
                // user for role is now deselected previously he is in role(3)
                else if (!userRoleViewModels[i].IsSelected && (await _userManager.IsInRoleAsync(identityUser, identityRole.Name)))
                {
                    identityResult = await _userManager.RemoveFromRoleAsync(identityUser, identityRole.Name);
                }
                // user for roles not changed
                else
                {
                    continue;
                }

                // after make the changes,if count < list.count then continue
                if (identityResult.Succeeded)
                {
                    // check we reached the end of adding user to role or not,if it full redirect editrole,else continue to add user to role
                    if (i < userRoleViewModels.Count - 1)
                    {
                        continue;
                    }
                    else
                    {
                        return RedirectToAction("EditRole", new { id = roleId });
                    }

                }
            }
            // Else redirect to editrole
            return RedirectToAction("EditRole", new { Id = roleId });
        }


        // When delete button is clicked we need to delete role.
        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        // in order to acces the delete role user need to have a claim specified by policy.
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var identityRole = await _roleManager.FindByIdAsync(roleId);

            if (identityRole == null)
            {
                // here we want return notfound view
                ViewBag.Message = $"The Role with Id = {roleId} Cannot Be Found,it already deleted.";
                return View("~/Views/Error/NotFound.cshtml");
            }

            try
            {
                var result = await _roleManager.DeleteAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return RedirectToAction("ListRoles");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"The exception occured : {ex}");
                ViewBag.ErrorTitle = $"{identityRole.Name} role is in use";
                ViewBag.ErrorMessage = $"{identityRole.Name} role cannot be deleted as there are users in this role. If you want to delete this role, please remove the users from the role and then try to delete";
                return View("~/Views/Error/Error.cshtml");
            }

        }




        /* ---------------------------------------------------- Manage Identity-ApplicationUsers ----------------------------------------------------*/

        // In order to diplay a list of all user we will routed here
        [HttpGet]
        public IActionResult ListUsers()
        {
            // In order to get all registed identity user(ApplicationUser) we need to make use of userManger.Users property
            var applicationUsers = _userManager.Users.ToList();

            // And pass that model for view
            return View(applicationUsers);
        }

        // When user click on edit user button in listusers view
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            // Retrive user with id exist in databse or not
            var applicationUser = await _userManager.FindByIdAsync(id);

            //If User doesnot exists we issue NotFound view instead of default 
            if (applicationUser == null)
            {
                // here we want return notfound view
                ViewBag.Message = $"The User with Id = {id} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }

            // GetRolesAsync returns the list of user Roles
            var userRoles = await _userManager.GetRolesAsync(applicationUser);
            // GetClaimsAsync retunrs the list of user Claims
            var userClaims = await _userManager.GetClaimsAsync(applicationUser);

            // Create data for view
            EditUserViewModel editUserViewModel = new EditUserViewModel()
            {
                id = applicationUser.Id,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email,
                City = applicationUser.City,
                Roles = userRoles.ToList(),
                Claims = userClaims.Select(c => c.Type+" : "+c.Value).ToList()
            };



            // This was returning view with data for diaplay
            return View(editUserViewModel);
        }


        // when user submit the edit form
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel editUserViewModel)
        {
            // Retive user from userid
            var applicationUser = await _userManager.FindByIdAsync(editUserViewModel.id);

            //If User doesnot exists we issue NotFound view instead of default 
            if (applicationUser == null)
            {
                // here we want return notfound view
                ViewBag.Message = $"The User with Id = {editUserViewModel.id} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }
            else
            {
                // Update user in database
                applicationUser.Email = editUserViewModel.Email;
                applicationUser.UserName = editUserViewModel.UserName;
                applicationUser.City = editUserViewModel.City;

                var result = await _userManager.UpdateAsync(applicationUser);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(editUserViewModel);

            }


        }

        // This is to delete user from AspnetUsers table by using post request because if we get it mailicous email where image src attribute is set get requset to delete so to avoid this use post
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var applicationUser = await _userManager.FindByIdAsync(userId);

            if (applicationUser == null)
            {
                ViewBag.Message = $"The User with Id = {userId} Cannot Be Found,It might already been deleted";
                return View("~/Views/Error/NotFound.cshtml");
            }
            else
            {
                // Instead of provide the Exception to DeveloperExceptionpage middleware we just need to handle that and return custom error view it can be any environment also.
                try
                {
                    // We need to delete user
                    var result = await _userManager.DeleteAsync(applicationUser);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListUsers");
                    }
                    else
                    {
                        ModelState.AddModelError("Error:", "some error");
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return RedirectToAction("ListUsers");
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError($"The some exception occured : {ex}");
                    ViewBag.ErrorTitle = $"{applicationUser.UserName} user is in use";
                    ViewBag.ErrorMessage = $"{applicationUser.UserName} user cannot be deleted as there are roles in this user. If you want to delete this user, please remove the roles from the user and then try to delete";
                    return View("~/Views/Error/Error.cshtml");
                }
            }
        }

        // Here we get request from edituser view to edit or manage roles of user which same as EditUsersInRols()
        [HttpGet]
        // here we are protecting with the custom AuthorizationRequirement with policy
        [Authorize(Policy ="EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(userId);

            if (applicationUser == null)
            {
                ViewBag.Message = $"The User with Id = {userId} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }

            List<UserRolesViewModel> userRolesViewModels = new List<UserRolesViewModel>();

            foreach (var role in _roleManager.Roles.ToList())
            {
                // Create an instance of UserRolesViewModel and populate roleId and roleName
                UserRolesViewModel userRolesViewModel = new UserRolesViewModel()
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                // Here we check is user in this role is already applied by previously
                if (await _userManager.IsInRoleAsync(applicationUser, role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }

                userRolesViewModels.Add(userRolesViewModel);
            }

            // Here we are passing the userId using viewbag
            ViewBag.UserId = applicationUser.Id;
            ViewBag.UserName = applicationUser.UserName;


            return View(userRolesViewModels);
        }

        [HttpPost]
        [Authorize(Policy ="EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> userRolesViewModels, string userId)
        {
            var applicationUser = await _userManager.FindByIdAsync(userId);

            if (applicationUser == null)
            {
                ViewBag.Message = $"The User with Id = {userId} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }
            else
            {
                // Retrive all list of existing roles user having
                var roles = await _userManager.GetRolesAsync(applicationUser);
                // Remove all roles from user
                var result = await _userManager.RemoveFromRolesAsync(applicationUser, roles);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot remove user existing roles");
                    return View(userRolesViewModels);
                }
                // add new roles to user 
                result = await _userManager.AddToRolesAsync(applicationUser, userRolesViewModels.Where(x => x.IsSelected == true).Select(x => x.RoleName));

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot add selected roles to user");
                    return View(userRolesViewModels);
                }

                return RedirectToAction("EditUser", new { id = userId });

            }
        }

        // This is to manage user claims that is add or remove the claims,since to keep application clean we are using static claims
        [HttpGet]
        [Authorize(Policy ="EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var applicationUser = await _userManager.FindByIdAsync(userId);

            if (applicationUser == null)
            {
                ViewBag.Message = $"The User with Id = {userId} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }

            // UserManager service GetClaimsAsync method gets all the current claims of the user
            var existingUserClaims = await _userManager.GetClaimsAsync(applicationUser);

            // Create a Model for view
            var userClaimsViewModel = new UserClaimsViewModel()
            {
                UserId = userId
            };

            // Loop through each claim we have in our application(claimstore)
            foreach (var claim in ClaimsStore.AllClaims)
            {
                // add claim type to viewmodel to make avaliable clauims to display
                UserClaim userClaim = new UserClaim()
                {
                    ClaimType = claim.Type
                };

                //  If the user has the claim, set IsSelected property to true, so the checkbox next to the claim is checked on the UI
                if (existingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true"))
                {
                    userClaim.IsSelected = true;
                }

                userClaimsViewModel.Claims.Add(userClaim);
            }

            return View(userClaimsViewModel);
        }

        [HttpPost]
        [Authorize(Policy ="EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel userClaimsViewModel)
        {
            var applicationUser = await _userManager.FindByIdAsync(userClaimsViewModel.UserId);

            if (applicationUser == null)
            {
                ViewBag.Message = $"The User with Id = {userClaimsViewModel.UserId} Cannot Be Found";
                return View("~/Views/Error/NotFound.cshtml");
            }

            // retrive all user existing claims 
            var existingUserClaims = await _userManager.GetClaimsAsync(applicationUser);

            // let us delete all existing claims of user
            var result = await _userManager.RemoveClaimsAsync(applicationUser,existingUserClaims);

            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing Claims");
                return View(userClaimsViewModel);
            }
            // add selected claims on UI available in model to user
            result = await _userManager.AddClaimsAsync(applicationUser,userClaimsViewModel.Claims.Select(c=> new Claim(c.ClaimType,c.IsSelected ? "true":"false")));

             if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                return View(userClaimsViewModel);
            }

            return RedirectToAction("EditUser",new { id=userClaimsViewModel.UserId });
        }

         // This is place user is been redirected to this when he try to access resource he donot have permission to that.
        // Example if user is not in admin and try to access the ManageRoles action by knowing link Administration/ListRoles,since he is bben denied so he redirected to Account/AccessDenied()
        // since now we change the deafult AccessDenied path from Account/AccessDenied to Administration/AccessDenied.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }

}