using MentalHealth_BackEnd.helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentalHealth_BackEnd.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; } 
        public virtual User User { get; set; }
        [ForeignKey("TherapistAndDoctor")]
        public string TherapistId { get; set; } 
        public virtual TherapistAndDoctor TherapistAndDoctor { get; set; }
        public DateTime AppointmentDate { get; set; } 
        public string Notes { get; set; } 
        public StatusOfAppointment Status { get; set; } // e.g., "Pending", "Confirmed", "Cancelled"

    }
}
