using EmployeeManagementApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EmployeeManagementApp.Data 
{
    // To Work with Identity Framework make class Derive from IdentityDbContext class becuse this class provide all Entity-Sets related to manage identity tables in Database .
    // In order to specify IdentityDbContext class  it has to work with our custom user class (in this case ApplicationUser class) instead of the default built-in IdentityUser class.  Then in migration Up() and Down() method will have code.
    public class AppDbContext : IdentityDbContext<ApplicationUser> 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Employee> Employees {get;set;}

        // In order to seed the Data to Entity override this.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call Extension method that  we written in ModelBuilderExtensionSeed
            modelBuilder.Seed();

            base.OnModelCreating(modelBuilder);

            // Here we need to casacde delete behavoiour set no delete
            // By default EF core will set delete behaviour of parent row is set to casacde the child row thsi is we delete key parent to which existsing foregin key of child are poninting makes the child keys/rows delete
            // This default behavoiur we are making as rextrict, so if try to delete key to which FK are poiniting server will do some actions like throwing exception.this is done by setting restrict on FK instead of cascade
            // Instead of returinning the exception we need to return Custom error view
            foreach(var foreginKey in modelBuilder.Model.GetEntityTypes().SelectMany(x=>x.GetForeignKeys()))
            {
                foreginKey.DeleteBehavior = DeleteBehavior.Restrict;
            }            

        }

    }
}