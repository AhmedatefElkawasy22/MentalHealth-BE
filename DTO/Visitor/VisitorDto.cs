namespace MentalHealth.DTOs
{
    public class VisitorDto
    {
        public int VisitorId { get; set; }
        public string UserName { get; set; }
        public string password {  get; set; }
        public string FullName { get; set; }
        public string Email { get; set; } 
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
