using System.ComponentModel.DataAnnotations;

namespace HFile.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(50, ErrorMessage = "Full Name cannot be longer than 50 characters")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("Male|Female|Other", ErrorMessage = "Gender must be Male, Female, or Other")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [StringLength(15)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        [StringLength(250)]
        public string? Imageprofile { get; set; }  // Optional, DB me path store karenge
    }
}
