using MentalHealth_BackEnd.helpers;

namespace MentalHealth_BackEnd.DTO.React_Comment
{
    public class AddReactDTO
    {
        public int PostId { get; set; }
        public DateTime ReactedAt { get; set; } = DateTime.UtcNow;
        public TypeOfReact ReactionType { get; set; } 
    }
}
