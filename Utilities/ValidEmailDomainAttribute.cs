using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApp.Utilities
{


    // Here we are cretaing class that acts custom validation attribute
    // Fisrt dervie class from Abstarcat ValidationAttribute after that override the IsValid() of that
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        // Since it inherits from base class so ErrorMessage... etc propertys are coming from those base class
        private string _allowedDoamin;

        // This is to take allowedDomain  property value instead using hard code,and value for this will came from the place where this attribute is used
        public ValidEmailDomainAttribute(string allowedDoamin)
        {
            _allowedDoamin = allowedDoamin;
        }

        // Through model binding we receive value for Email in Email property since we applied this custom attribute on Email property of register view model so this object will get value of email
        public override bool IsValid(object value)
        {
            // Splict email at place @ symbol and check is allowedDomain == domain that user used.
            string[] arr = value.ToString().Split('@');
            // we know enterd domain is @1 index
            var result =  arr[1].ToUpper().Equals(_allowedDoamin.ToUpper());
            return result;
        }
    }

}