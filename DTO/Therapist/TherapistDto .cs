namespace MentalHealth_BackEnd.DTO.Therapist
{
    public class TherapistDto
    {
        public int TherapistId { get; set; } // Primary key
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string SpecializationName { get; set; }

    }
}
