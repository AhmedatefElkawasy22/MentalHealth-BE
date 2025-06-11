using MentalHealth_BackEnd.DTO.Post;
using MentalHealth_BackEnd.DTO.React_Comment;
using MentalHealth_BackEnd.helpers;
using MentalHealth_BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MentalHealth_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUrlHelperService _urlHelperService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        public PostController(ApplicationDbContext context, IUrlHelperService urlHelperService, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _context = context;
            _urlHelperService = urlHelperService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        [HttpGet("GetAllPosts"), Authorize]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _context.posts.Include(e => e.User).Include(p => p.comments).Include(p => p.reacts).ToListAsync();
            if (posts == null || !posts.Any())
                return NotFound("No posts found.");
            return Ok(posts.Select(p => new
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                ImagePath = p.ImagePath,
                UserName = p.User.UserName,
                CommentsCount = p.comments.Count,
                ReactsCount = p.reacts.Count,
            }));
        }

        [HttpPost("AddPost"), Authorize(Roles = "Therapist,Doctor")]
        public async Task<IActionResult> AddPostAsync([FromForm] AddPost data)
        {
            if (!ModelState.IsValid)
                return BadRequest("please enter a valid data");

            try
            {
                string imageDirectory = Path.Combine("wwwroot", "Images_of_Posts");

                if (!Directory.Exists(imageDirectory))
                    Directory.CreateDirectory(imageDirectory);

                List<string> invalidFiles = new();
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

                var extension = Path.GetExtension(data.Image.FileName)?.ToLowerInvariant();

                if (data.Image.Length <= 5 * 1024 * 1024 && allowedExtensions.Contains(extension))
                {
                    string fileName = $"{data.Title}_{Guid.NewGuid()}.jpg";
                    string filePath = Path.Combine(imageDirectory, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await data.Image.CopyToAsync(stream);
                    }

                    string imageUrl = $"{_urlHelperService.GetCurrentServerUrl()}/Images_of_Posts/{fileName}";


                    _context.posts.Add(new Post
                    {
                        Title = data.Title,
                        Content = data.Content,
                        CreatedAt = DateTime.Now,
                        ImagePath = imageUrl,
                        UserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    });

                    await _context.SaveChangesAsync();

                }
                else
                {
                    return BadRequest("Invalid file format or size. Please upload a valid image file (png, jpg, jpeg) with a maximum size of 5MB.");
                }

                return Ok("Post added successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while adding the post: {ex.Message}");

            }
        }

        [HttpGet("GetPostById/{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var post = await _context.posts.Include(p => p.comments).Include(p => p.reacts).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
                return NotFound("Post not found.");

            var result = new
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                ImagePath = post.ImagePath,
                UserName = post.User.UserName,
                CommentsCount = post.comments?.Count ?? 0,
                ReactsCount = post.reacts?.Count ?? 0
            };

            return Ok(result);
        }


        [HttpDelete("DeleteImage/{id}"), Authorize(Roles = "Therapist,Doctor")]
        public async Task<IActionResult> DeleteImageOfPost(int id)
        {
            var post = await _context.posts.Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound("Post not found.");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (post.UserId != currentUserId)
                return Forbid("You are not authorized to delete this post's image.");

            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var imagePath = Path.Combine("wwwroot", "Images_of_Posts", Path.GetFileName(post.ImagePath));
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
                post.ImagePath = null;
                await _context.SaveChangesAsync();
            }

            return Ok("Image deleted successfully.");
        }


        [HttpPut("EditImage/{id}"), Authorize(Roles = "Therapist,Doctor")]
        public async Task<IActionResult> EditImageOfPost(int id, IFormFile newImage)
        {
            var post = await _context.posts.Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound("Post not found.");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (post.UserId != currentUserId)
                return Forbid("You are not authorized to edit this post's image.");

            if (newImage.Length > 5 * 1024 * 1024) return BadRequest("File size must not exceed 5MB.");
            var allowed = new[] { ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(newImage.FileName)?.ToLowerInvariant();
            if (!allowed.Contains(ext)) return BadRequest("Invalid file type.");

            // remove old
            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var old = Path.Combine("wwwroot", "Images_of_Posts", Path.GetFileName(post.ImagePath));
                if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
            }

            var newName = $"{post.Title}_{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine("wwwroot", "Images_of_Posts", newName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await newImage.CopyToAsync(stream);
            post.ImagePath = $"{_urlHelperService.GetCurrentServerUrl()}/Images_of_Posts/{newName}";
            await _context.SaveChangesAsync();

            return Ok("Image updated successfully.");
        }


        [HttpDelete("DeletePost/{id}"), Authorize(Roles = "Therapist,Doctor,Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.posts.Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound("Post not found.");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (post.UserId != currentUserId && !isAdmin)
                return Forbid("You are not authorized to delete this post.");

            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var imagePath = Path.Combine("wwwroot", "Images_of_Posts", Path.GetFileName(post.ImagePath));
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            _context.posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok("Post deleted successfully.");
        }


        [HttpPut("EditPost/{id}"), Authorize(Roles = "Therapist,Doctor")]
        public async Task<IActionResult> EditPost(int id, [FromBody] AddPost updated)
        {
            var post = await _context.posts.Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound("Post not found.");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (post.UserId != currentUserId)
                return Forbid("You are not authorized to edit this post.");

            post.Title = updated.Title;
            post.Content = updated.Content;
            await _context.SaveChangesAsync();
            return Ok("Post updated successfully.");
        }


        [HttpPost("AddReact"), Authorize]
        public async Task<IActionResult> AddReact([FromBody] AddReactDTO react)
        {
            if (!ModelState.IsValid)
                return BadRequest("InvalidData , Please provide valid data for the reaction.");

            _context.ReactsOnPosts.Add(new ReactOnPost
            {
                PostId = react.PostId,
                ReactedAt = react.ReactedAt,
                ReactionType = react.ReactionType,
                UserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            });

            await _context.SaveChangesAsync();
            return Ok("React added successfully.");
        }

        [HttpPost("AddComment"), Authorize]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDTO comment)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data, please provide valid data for the comment.");

            _context.CommentsOnPosts.Add(new CommentOnPost
            {
                PostId = comment.PostId,
                Content = comment.Content,
                DateCreated = comment.CommentedAt,
                UserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            await _context.SaveChangesAsync();
            return Ok("Comment added successfully.");
        }

        [HttpGet("GetReactsByPostId/{postId}"), Authorize]
        public async Task<IActionResult> GetReactsByPostId(int postId)
        {
            var reacts = await _context.ReactsOnPosts
                .Where(r => r.PostId == postId)
                .Include(r => r.User)
                .ToListAsync();
            if (reacts == null || !reacts.Any())
                return NotFound("No reactions found for this post.");
            return Ok(reacts.Select(r => new
            {
                r.Id,
                r.ReactionType,
                r.ReactedAt,
                UserName = r.User.UserName
            }));
        }

        [HttpGet("GetCommentsByPostId/{postId}"), Authorize]
        public async Task<IActionResult> GetCommentsByPostId(int postId)
        {
            var comments = await _context.CommentsOnPosts
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .ToListAsync();
            if (comments == null || !comments.Any())
                return NotFound("No comments found for this post.");
            return Ok(comments.Select(c => new
            {
                c.Id,
                c.Content,
                c.DateCreated,
                UserName = c.User.UserName
            }));
        }


        [HttpDelete("DeleteReact/{id}"), Authorize]
        public async Task<IActionResult> DeleteReact(int id)
        {
            ReactOnPost? react = await _context.ReactsOnPosts
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (react == null)
                return NotFound("Reaction not found.");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (react.UserId != currentUserId)
                return Forbid("You are not authorized to delete this reaction.");

            _context.ReactsOnPosts.Remove(react);
            await _context.SaveChangesAsync();
            return Ok("Reaction deleted successfully.");
        }

        [HttpDelete("DeleteComment/{id}"), Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.CommentsOnPosts
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
                return NotFound("Comment not found.");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.UserId != currentUserId)
                return Forbid("You are not authorized to delete this comment.");

            _context.CommentsOnPosts.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok("Comment deleted successfully.");
        }

        [HttpPut("EditComment/{id}"), Authorize]
        public async Task<IActionResult> EditComment(int id, [FromBody] UpdateCommentDTO updated)
        {
            var comment = await _context.CommentsOnPosts
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
                return NotFound("Comment not found.");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.UserId != currentUserId)
                return Forbid("You are not authorized to edit this comment.");

            comment.Content = updated.Content;
            await _context.SaveChangesAsync();
            return Ok("Comment updated successfully.");
        }

    }
}
