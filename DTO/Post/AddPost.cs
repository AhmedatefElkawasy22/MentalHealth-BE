using System.ComponentModel.DataAnnotations;

namespace MentalHealth_BackEnd.DTO.Post
{
    public class AddPost
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(150, ErrorMessage = "Title must be at most 150 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [MaxLength(5000, ErrorMessage = "Content must be at most 5000 characters")]
        public string Content { get; set; }

        public IFormFile? Image { get; set; }
    }
}
