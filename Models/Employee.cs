using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementApp.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }  
          
        [Required]
        public Department? Department { get; set; } 
        public string PhotoPath {get;set;} 

         // Here we need to send EmployeeId in the form of Encrypted so add one field to hold encrypted value and exclude mapping it with database column
        [NotMapped]
        public string EncryptedId { get; set; }
        
    }
}