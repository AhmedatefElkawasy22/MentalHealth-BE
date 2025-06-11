using System.ComponentModel.DataAnnotations.Schema;

namespace MentalHealth_BackEnd.Models
{
    public class CommentOnPost
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime DateCreated { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        [ForeignKey("Post")]
        public int PostId { get; set; } 
        public virtual User User { get; set; }
        public virtual Post Post { get; set; }
    }
}
