using EmployeeManagementApp.Models;
using EmployeeManagementApp.Repositories;
using EmployeeManagementApp.Security;
using EmployeeManagementApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;


namespace EmployeeManagementApp.Controllers
{
   
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private IEmployeeRepository _employeeRepository;

        [Obsolete]
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;

        // To be able to encrypt we need instance of IDataProtector type,this will have a Protect() and UnProtect() used to encrypt and decrypt.
        // To get instance of  IDataProtector type ,inject an IDataProtectionProvider framework services[as shown below] and call CreateProtector("Puprposestring")
        public IDataProtector protector;

        [Obsolete]
        public HomeController(IEmployeeRepository employeeRepository, Microsoft.AspNetCore.Hosting.IHostingEnvironment env,IDataProtectionProvider dataProtectionProvider,DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {

            _employeeRepository = employeeRepository;
            _hostingEnvironment = env;

            // In oredr get an IDataProtector that can used to encrypt and Decrypt we are injecting IDataProtectionProvider
            // Inorder to get an puprose string inject DataProtectionPurposeStrings instance ,so that we can use in dataProtectionProvider.CreateProtector()
            protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValues);
        }

        // [HttpGet]
        // [Route("{GetEmployee}/{id}")]
        // [Route("~/")] 
        // [Route("/index")]
        // public IActionResult Index([FromRoute]int id)
        // {
        //     // this is Manaually way of getting services.
        //     // var services = HttpContext.RequestServices;
        //     // var service = (IEmployeeRepository)services.GetService(typeof(IEmployeeRepository));
        //     // var emp =  service.GetEmployee(1);

        //     var employee = _employeeRepository.GetEmployee(id);
        //     var pagetitle = "Employee Details";     

        //     var employeeViewModel = new HomeDetailsViewModel()
        //     {
        //         EmployeeModel = employee,
        //         PageTitle = pagetitle
        //     };

        //     return View(employeeViewModel);
        // }

        [Route("[action]")]
        [Route("~/")]
        public ViewResult Index()
        {
            // Here we need to populate value for EncryptedId for each employee using protector.Protect() by using Id of employee to encrypt
            var model = _employeeRepository.GetEmployees().Select(e=>
            {
                // populate value for employee and send use employee
                e.EncryptedId = protector.Protect(e.Id.ToString());
                return e;
            });
            return View(model);
        }

        // Details view receives the encrypted employee ID
        [Route("[action]/{id?}")]
        public ViewResult Details(string id)
        {
          
            // Decrypt the employee id using Unprotect method
            string decryptedId = protector.Unprotect(id);
            int decryptedEmployeetId = Convert.ToInt32(decryptedId);

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = _employeeRepository.GetEmployee(decryptedEmployeetId),
                PageTitle = "Employee Details"
            };
            if(homeDetailsViewModel.Employee == null)
            {
                //Type 1-404: The Resource with specific Id not exist,we have issue 404 error Page.
                Response.StatusCode = 404;
                return View("404EmpNotFound",id);
            }
            homeDetailsViewModel.Employee.EncryptedId = protector.Protect(homeDetailsViewModel.Employee.Id.ToString());
            return View(homeDetailsViewModel);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        [Route("Create")]
        [Obsolete]
        public IActionResult Create(CreateEmployeeViewModel newEmpToAdd)
        {
            // Once User has upload the phot we need store path of photo in Database and Photo in webroot folder of local server
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                var employee = new Employee();

                // If the Photo property on the incoming model object is not null, then the user has selected an image to upload.
                if (newEmpToAdd.Photo != null)
                {
                    // The image must be uploaded to the images folder in wwwroot
                    // To get the path of the wwwroot folder we are using the inject
                    // HostingEnvironment service provided by ASP.NET Core
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");


                    // To make sure the file name is unique we are appending a new GUID value and and an underscore to the file name
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + newEmpToAdd.Photo.FileName;

                    // Generate file path to store in Database
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    //Use CopyTo() method provided by IFormFile interface to copy the file to wwwroot/images folder
                    // Since it excepts stream lets open stream on path
                    newEmpToAdd.Photo.CopyTo(new FileStream(filePath, FileMode.Create));


                    // convert the viewmodel to your db entity
                    employee.PhotoPath = uniqueFileName;
                }
                employee.Name = newEmpToAdd.Name;
                employee.Email = newEmpToAdd.Email;
                employee.Department = newEmpToAdd.Department;

                

                _employeeRepository.AddEmployee(employee);
                // here we are encrypting id 
                employee.EncryptedId = protector.Protect(employee.Id.ToString());
                return RedirectToAction("Details", new { id = employee.EncryptedId});
            }
            return View("Create");
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            string decryptedId = protector.Unprotect(id);
            int decryptedEmployeetId = Convert.ToInt32(decryptedId);

            var employee = _employeeRepository.GetEmployee(decryptedEmployeetId);
            if(employee == null)
            {
                 Response.StatusCode = 404;
                return View("404EmpNotFound",decryptedEmployeetId);
            }
            var editEmployeeViewModel = new EditEmployeeViewModel(){
                Id = protector.Protect(employee.Id.ToString()),
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
        
            return View(editEmployeeViewModel);
        }

        [HttpPost]
        [Obsolete]
        public IActionResult Edit(EditEmployeeViewModel editEmployee)
        {
            if (ModelState.IsValid)
            {
                string decryptedId = protector.Unprotect(editEmployee.Id);
                int decryptedEmployeetId = Convert.ToInt32(decryptedId);

                var employee = _employeeRepository.GetEmployee(decryptedEmployeetId);
                if (editEmployee.Photo != null)
                {
                    if (employee.PhotoPath != null)
                    {
                        string photoPathToDelete = Path.Combine(_hostingEnvironment.WebRootPath, "images", employee.PhotoPath);
                        System.IO.File.Delete(photoPathToDelete);
                    }
                    employee.PhotoPath = UploadPhotoToLocalServer(editEmployee);
                }
                employee.Name = editEmployee.Name;
                employee.Email = editEmployee.Email;
                employee.Department = (Department)editEmployee.Department;

                var updateEmployee = _employeeRepository.UpdateEmployee(employee);
                return RedirectToAction("index");
            }
            return View(editEmployee);
        }
        

        [Obsolete]
        private string UploadPhotoToLocalServer(EditEmployeeViewModel employee)
        {
            string uniqueFileName = null;
            if (employee.Photo != null)
            {
                uniqueFileName = Guid.NewGuid() + "_" + employee.Photo.FileName;
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                //  This is to Close filestream once the file as uploaded
                using (var filestream = new FileStream(filePath, FileMode.Create))
                {
                    employee.Photo.CopyTo(filestream);
                }

            }
            return uniqueFileName;
        }

       [HttpPost]
       [Route("[action]/{id}")]
        [Obsolete]
        public IActionResult Delete(string id)
        {
            // decrypt user Id
            var decryptedId = protector.Unprotect(id);
            int decryptedEmployeetId = Convert.ToInt32(decryptedId);

            var employee = _employeeRepository.Delete(decryptedEmployeetId);
            if(employee == null)
            {
                return View("~/Views/Home/CannotDelete.cshtml");
            }
            // here we need delete his photo from our server also is his photo path exists
            if(!string.IsNullOrEmpty(employee.PhotoPath))
            {
                string photoPathToDelete = Path.Combine(_hostingEnvironment.WebRootPath, "images", employee.PhotoPath);
                System.IO.File.Delete(photoPathToDelete);   
            }
            return View("DeleteConfirmationEmployee",employee);
        }
    }
}