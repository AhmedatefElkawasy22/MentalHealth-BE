namespace MentalHealth_BackEnd.Models
{
    public class Specialization
    {
       
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TherapistAndDoctor> Therapists  { get; set; }

    }
}
