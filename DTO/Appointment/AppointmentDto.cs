namespace MentalHealth_BackEnd.DTO.Appointment
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public string VisitorName { get; set; }
        public string TherapistName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
    }
}
