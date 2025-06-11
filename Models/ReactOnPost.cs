using MentalHealth_BackEnd.helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentalHealth_BackEnd.Models
{
    public class ReactOnPost
    {
        public int Id { get; set; }
        [ForeignKey("Post")]
        public int PostId { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; } 
        public DateTime ReactedAt { get; set; } 
        public TypeOfReact ReactionType { get; set; } 

        public User User { get; set; }
        public Post Post { get; set; }
    }
}
