using MentalHealth_BackEnd.helpers;
using System.ComponentModel.DataAnnotations;

namespace MentalHealth_BackEnd.DTO.Auth
{
    public class RegisterUserDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [RegularExpression(@"^\+\d{8,15}$", ErrorMessage = "Phone number must start with '+' followed by 8 to 15 digits.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords don't match.")]
        public string ConfirmPassword { get; set; }

        [MaxLength(100, ErrorMessage = "Address must be less than 100 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime BornDate { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public Gender gender { get; set; }

        public IFormFile ProfilePicture { get; set; }
    }
}
