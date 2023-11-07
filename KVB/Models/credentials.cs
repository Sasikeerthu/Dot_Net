using System.ComponentModel.DataAnnotations;

namespace KVB.Models
{
    public class credentials
    {

        public string name { get; set; } = string.Empty;
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
        public string emailid { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;

        [Compare("password", ErrorMessage = "Password and Confirm Password do not match.")]
        public string Confirm_password { get; set; } = string.Empty;
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        public string phone { get; set; } = string.Empty;

    }
    
}
