using MentalHealth_BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentalHealth_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        public UserController(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }


        [HttpGet("GetDataOfCurrentUser")]
        [Authorize]
        public async Task<IActionResult> GetDataOfCurrentUserAsync()
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("User is not authenticated.");

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return NotFound("User not found.");

            if (currentUser is TherapistAndDoctor t)
            {
                var certificates = t.certificates?.Select(e => e.Path).ToList() ?? new List<string>();
                var specializationName = t.Specialization?.Name ?? "Unknown";

                return Ok(new
                {
                    Id = currentUserId,
                    Name = t.Name,
                    Email = t.Email,
                    LicencePath = t.PathOfLicence,
                    Address = t.Address,
                    City = t.City,
                    BornDate = t.BornDate,
                    Gender = t.gender,
                    ProfilePicture = t.pathProfilePicture,
                    Phone = t.PhoneNumber,
                    CertificatePaths = certificates,
                    SpecializationName = specializationName
                });
            }

            if (currentUser is User u)
            {
                return Ok(new
                {
                    Id = currentUserId,
                    Name = u.Name,
                    Email = u.Email,
                    Address = u.Address,
                    BornDate = u.BornDate,
                    Gender = u.gender,
                    ProfilePicture = u.pathProfilePicture,
                    Phone = u.PhoneNumber
                });
            }

            return BadRequest("Unsupported user type.");
        }

    }
}
