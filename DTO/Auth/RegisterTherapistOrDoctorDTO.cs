using MentalHealth_BackEnd.helpers;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MentalHealth_BackEnd.DTO.Auth
{
    public class RegisterTherapistOrDoctorDTO : RegisterUserDTO
    {
        [Required(ErrorMessage = "License file is required.")]
        public IFormFile License { get; set; }

        [Required(ErrorMessage = "Specialization ID is required.")]
        public int SpecializationID { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100, ErrorMessage = "City name cannot exceed 100 characters.")]
        public string City { get; set; }
        [Required(ErrorMessage = "role is required.")]
        public Role role { get; set; }
    }
}
