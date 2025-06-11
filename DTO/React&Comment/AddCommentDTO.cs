namespace MentalHealth_BackEnd.DTO.React_Comment
{
    public class AddCommentDTO
    {
        public int PostId { get; set; }
        public string Content { get; set; }
        public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
    }
}
