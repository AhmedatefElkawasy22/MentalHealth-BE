using System.ComponentModel.DataAnnotations.Schema;

namespace MentalHealth_BackEnd.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImagePath { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public List<ReactOnPost> reacts { get; set; }
        public List<CommentOnPost> comments { get; set; }
    }
}
