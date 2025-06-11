using MentalHealth_BackEnd.helpers;

namespace MentalHealth.DTOs
{
    public class UpdateVistorDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime BornDate { get; set; }
        public Gender gender { get; set; }
    }
}
