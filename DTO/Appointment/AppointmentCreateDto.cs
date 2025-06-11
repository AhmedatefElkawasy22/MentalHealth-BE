namespace MentalHealth_BackEnd.DTO.Appointment
{
    public class AppointmentCreateDto
    {
        public string TherapistId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Notes { get; set; }
    }
}
