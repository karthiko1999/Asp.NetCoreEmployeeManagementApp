using EmployeeManagementApp.Models;

// This repository interface represents what are operations and data need & returned for Any Datasource
namespace EmployeeManagementApp.Repositories
{
    public interface IEmployeeRepository
    {
        Employee GetEmployee(int id);
        List<Employee> GetEmployees();

        Employee AddEmployee(Employee emp);

        Employee UpdateEmployee(Employee emp);

        Employee Delete(int id);
    }
}