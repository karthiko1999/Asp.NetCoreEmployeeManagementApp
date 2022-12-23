using Microsoft.AspNetCore;
using NLog.Extensions.Logging;

namespace EmployeeManagement
{
    public class Program 
    {
        public static void Main(string[] args)
        {
             CreateWebHostBuilder(args).Build().Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
        // .ConfigureLogging((hostingContext, logging) =>
        // {
        //     // logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        //     // logging.AddConsole();
        //     // logging.AddDebug();
        //     // logging.AddEventSourceLogger();
        //     // Enable NLog as one of the Logging Provider
        //     // logging.AddNLog();
        // })
        .UseStartup<MyStartup>();
    }
    
}
