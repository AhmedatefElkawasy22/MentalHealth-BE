using System.ComponentModel.DataAnnotations.Schema;

namespace MentalHealth_BackEnd.Models
{
    public class Certificate
    {
        public int Id { get; set; }
        public string Path { get; set; }

        [ForeignKey("TherapistAndDoctor")]
        public string TherapistAndDoctorId { get; set; }
        public TherapistAndDoctor TherapistAndDoctor { get; set; }
    }
}
