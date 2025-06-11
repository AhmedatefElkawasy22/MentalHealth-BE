using System.ComponentModel.DataAnnotations;

namespace MentalHealth_BackEnd.DTO.Auth
{
    public class LoginUserDTO
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
