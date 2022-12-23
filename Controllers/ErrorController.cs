using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace EmployeeManagementApp.Controllers
{
    public class ErrorController : Controller
    {
        // In order log the exception to Debug window or Console window or file use ILogger service. 
        private readonly ILogger<ErrorController> _logger;

        // Inject ASP.NET Core ILogger service. Specify the Controller Type as the generic parameter. This helps us identify later which class or controller has logged the exception
        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }




        //Type 2-404: The URL doesnot matches with any Route.
        // If there is 404 status code, the route path will become Error/404
        [Route("Error/{statuscode}")]
        public IActionResult HttpNonSuccessStatusCodeHandler(int statuscode)
        {
            switch (statuscode)
            {
                case 404:
                    ViewBag.Message = "Sorry, the resource you requested could not be found";

                    // To get all information regarding status code of pages use IStatusCodeReExecuteFeature on Get<>() of Feature property of HttpContext class
                    var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            

                    // _logger.LogTrace("Trace Log");
                    // _logger.LogDebug("Debug Log");
                    // _logger.LogInformation("Information Log");
                    // _logger.LogError("Error Log");
                    // _logger.LogCritical("Critical Log");
                    // Here we are logging the 404 Not Found Status Code Under Warning Category.
                    // LogWarning() method logs the unsucces Statuscode under Error category in the log
                    _logger.LogWarning($"{statuscode} Error occured in Path : {statusCodeReExecuteFeature.OriginalPath} & query string : {statusCodeReExecuteFeature.OriginalQueryString}");
                    break;

            }
            return View("NotFound");
        }


        // This the Route to return custom error message
        [Route("[action]")]
        public IActionResult Error()
        {
            // Retrieve the exception Details the occured using the HttpContext that ask what feature to return = IExceptionHandlerPathFeature
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            // ViewBag.Path = exceptionHandlerPathFeature.Path;
            // ViewBag.ErrorMessage = exceptionHandlerPathFeature.Error.Message;
            // ViewBag.StackTrace = exceptionHandlerPathFeature.Error.StackTrace;

            // _logger.LogTrace("Trace Log");
            // _logger.LogDebug("Debug Log");
            // _logger.LogInformation("Information Log");
            // _logger.LogWarning("Warning Log");
            // _logger.LogCritical("Critical Log");
            // Here we are Logging Exception under Error Category
            // LogError() method logs the exception under Error category in the log
            // _logger.LogError($"The Path {exceptionHandlerPathFeature.Path} threwed an Exception {exceptionHandlerPathFeature.Error}");

            return View("Error");
        }
    }
}