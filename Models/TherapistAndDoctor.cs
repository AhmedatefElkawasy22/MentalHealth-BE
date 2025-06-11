using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentalHealth_BackEnd.Models
{
    public class TherapistAndDoctor : User
    {
        public string? PathOfLicence { get; set; }
        [ForeignKey("Specialization")]
        public int SpecializationID { get; set; }
        public virtual Specialization Specialization { get; set; }
        public DateTime DateJoined { get; set; }
        public string City { get; set; }
        public bool IsApproved { get; set; }
        public List<Certificate> certificates { get; set; }
    }
}
