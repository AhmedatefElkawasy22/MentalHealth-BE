using MentalHealth_BackEnd.helpers;

namespace MentalHealth_BackEnd.DTO.React_Comment
{
    public class AddReactDTO
    {
        public int PostId { get; set; }
        public TypeOfReact ReactionType { get; set; } 
    }
}
