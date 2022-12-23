using EmployeeManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementApp.Data
{
    // Since it having an Extension methos ,so it must non-genric and non-nested static class.
    public static class ModelBuilderExtensionSeed
    {
        // Extension method are static method they are invoked just like instance.this used to extend the existing types or our custom types
        // This is Extension Method on ModelBuilder to keep our DbContext clean.
           public static void Seed(this ModelBuilder modelBuilder)
           {

               // Inorder to configure & seed data to particular entity use HasData().
               modelBuilder.Entity<Employee>().HasData(
                new Employee()
                {
                    Id = 1,
                    Name = "Sam",
                    Department = Department.IT,
                    Email = "sam@user.com"
                },
                new Employee()
                {
                    Id = 2,
                    Name = "Otto",
                    Department = Department.Payroll,
                    Email = "otto@user.com"
                },
                new Employee()
                {
                    Id = 3,
                    Name = "Alex",
                    Department = Department.HR,
                    Email = "alex123@user.com"
                },
                new Employee()
                {
                    Id = 4,
                    Name = "Henry",
                    Department = Department.None,
                    Email = "henry@user.com"
                }
               );
           }
           
    }
}
