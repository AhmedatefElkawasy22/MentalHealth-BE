using MentalHealth_BackEnd.helpers;
using Microsoft.AspNetCore.Identity;

namespace MentalHealth_BackEnd.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string pathProfilePicture { get; set; }
        public DateTime BornDate { get; set; }
        public Gender gender { get; set; }
    }
}
