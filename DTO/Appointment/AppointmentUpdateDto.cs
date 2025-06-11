using MentalHealth_BackEnd.helpers;

namespace MentalHealth_BackEnd.DTO.Appointment
{
    public class AppointmentUpdateDto
    {
        public DateTime AppointmentDate { get; set; }
        public string Notes { get; set; }
        public StatusOfAppointment Status { get; set; }
    }

}
