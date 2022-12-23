using EmployeeManagementApp.Models;

//Repository for an How the data is stored or retrived in InMemory DataSource
namespace EmployeeManagementApp.Repositories
{
    //  public class MockEmployeeRepository : IEmployeeRepository
    public class MockEmployeeRepository
    {
        List<Employee> _listemployees = null;

        public MockEmployeeRepository()
        {
            _listemployees = new List<Employee>(){

                new Employee()
                {
                    Id = 1,
                    Name = "Sam",
                    Department = Department.IT,
                    Email = "sam@micro.com"
                },
                new Employee()
                {
                    Id = 2,
                    Name = "Otto",
                    Department = Department.Payroll,
                    Email = "otto@micro.com"
                },
                new Employee()
                {
                    Id = 3,
                    Name = "Alex",
                    Department = Department.HR,
                    Email = "alex123@micro.com"
                }
            } ; 
        }

        public Employee GetEmployee(int id)
        {
            return _listemployees.FirstOrDefault(x=>x.Id == id);
        }

        public List<Employee> GetEmployees()
        {
            return _listemployees.ToList();
        }

        public Employee AddEmployee(Employee newEmpolyee)
        {
             newEmpolyee.Id = _listemployees.Count + 1;
             _listemployees.Add(newEmpolyee);
             return newEmpolyee;
        }

    }
}