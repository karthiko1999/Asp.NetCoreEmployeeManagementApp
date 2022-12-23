using EmployeeManagementApp.Data;
using EmployeeManagementApp.Models;

namespace EmployeeManagementApp.Repositories
{

    public class MySqlEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public MySqlEmployeeRepository(AppDbContext context)
        {
            this._context = context;
        }

        public List<Employee> GetEmployees()
        {
            return _context.Employees.ToList<Employee>();
        }
        public Employee GetEmployee(int id)
        {
            // The Find() of the Context class Used Find record by its primary key
            return _context.Find<Employee>(id);
        }
        public Employee AddEmployee(Employee emp)
        {
            emp.Id =  _context.Employees.Count<Employee>() + 1;
            _context.Employees.Add(emp);
            _context.SaveChanges();
            return emp;
        }

        public Employee UpdateEmployee(Employee emp)
        {
            // This using Attach() method of DbContext to update an entity without change state
            var empToUpdate = _context.Employees.Attach(emp);
            // Since Attach() doesont changes state of the above ,letus set EntityState modified manaually
            empToUpdate.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return emp;
        }

        public Employee Delete(int id)
        {
            var empToDelete = _context.Employees.Find(id);
            if(empToDelete != null)
            {
                _context.Employees.Remove(empToDelete);
                _context.SaveChanges();
            }
            
            return empToDelete;
        }
    }
}