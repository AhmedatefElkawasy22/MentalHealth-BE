using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MentalHealth_BackEnd.DTO;
using MentalHealth_BackEnd.Models;
using System.Threading.Tasks;
using MentalHealth_BackEnd.DTO.Therapist;
using MentalHealth_BackEnd.DTO.Specialization;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Claims;
using MentalHealth_BackEnd.helpers;
using Microsoft.AspNetCore.Identity;

namespace MentalHealth_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TherapistOrDoctorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUrlHelperService _urlHelperService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        public TherapistOrDoctorController(ApplicationDbContext context, IMapper mapper, IUrlHelperService urlHelperService, IHttpContextAccessor contextAccessor, UserManager<User> userManager)
        {
            _context = context;
            _mapper = mapper;
            _urlHelperService = urlHelperService;
            _httpContextAccessor = contextAccessor;
            _userManager = userManager;
        }
        [HttpGet("GetAllTherapists")]
        public async Task<IActionResult> GetAllTherapists()
        {
            var Therapists = await _context.TherapistsAndDoctors.Include(t => t.Specialization).Include(c => c.certificates).ToListAsync();
            var result = Therapists.Select(therapist => new
            {
                therapist.Email,
                therapist.Name,
                therapist.City,
                therapist.PhoneNumber,
                Specialization = new
                {
                    therapist.Specialization.Name,
                    therapist.Specialization.Description
                },
                therapist.PathOfLicence,
                therapist.gender,
                therapist.BornDate,
                therapist.DateJoined,
                therapist.pathProfilePicture,
                Certificates = therapist.certificates.Select(e => e.Path).ToList(),
            });
            return Ok(result);
        }

        // Get therapist by ID
        [HttpGet("GetTherapistById/{id}")]
        public async Task<IActionResult> GetTherapistById(string id)
        {
            var therapist = await _context.TherapistsAndDoctors.Include(t => t.Specialization).SingleOrDefaultAsync(i => i.Id == id);

            if (therapist == null)
                return NotFound();

            return Ok(new TherapistAndDoctor
            {
                Email = therapist.Email,
                Name = therapist.Name,
                City = therapist.City,
                PhoneNumber = therapist.PhoneNumber,
                Specialization = new Specialization { Name = therapist.Specialization.Name, Description = therapist.Specialization.Description },
                PathOfLicence = therapist.PathOfLicence,
                gender = therapist.gender,
                BornDate = therapist.BornDate,
                DateJoined = therapist.DateJoined,
            });
        }


        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] TherapistUpdateDto updateTherabist)
        {
            var therapist = await _context.TherapistsAndDoctors.FindAsync(id);
            if (therapist == null)
            {
                return NotFound(new { Message = "therapist not found" });
            }

            _mapper.Map(updateTherabist, therapist); // Map updated values to the existing entity
            _context.TherapistsAndDoctors.Update(therapist);
            await _context.SaveChangesAsync();

            return Ok(therapist);
        }


        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var therapist = await _context.TherapistsAndDoctors.FindAsync(id);
            if (therapist == null)
            {
                return NotFound(new { Message = "therapist not found" });
            }

            _context.TherapistsAndDoctors.Remove(therapist);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "therapist deleted successfully" });
        }


        [HttpPost("AddCertificate"), Authorize(Roles = "Therapist,Doctor")]
        public async Task<IActionResult> AddCertificateAsync([FromForm] UploadImageDTO data)
        {
            if (!ModelState.IsValid)
                return BadRequest("enter a valid iamge.");
            try
            {
                string imageDirectory = Path.Combine("wwwroot", "Images_of_Certificate");

                if (!Directory.Exists(imageDirectory))
                    Directory.CreateDirectory(imageDirectory);

                List<string> invalidFiles = new();
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
                var extension = Path.GetExtension(data.Image.FileName)?.ToLowerInvariant();

                if (data.Image.Length <= 5 * 1024 * 1024 && allowedExtensions.Contains(extension))
                {
                    var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var currentUser = await _userManager.FindByIdAsync(currentUserId);

                    string fileName = $"{currentUser.Name}_{Guid.NewGuid()}.jpg";
                    string filePath = Path.Combine(imageDirectory, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await data.Image.CopyToAsync(stream);
                    }

                    string imageUrl = $"{_urlHelperService.GetCurrentServerUrl()}/Images_of_Certificate/{fileName}";


                    _context.certificates.Add(new Certificate
                    {
                        Path = imageUrl,
                        TherapistAndDoctorId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
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

        [HttpPut("editCertificate/{id}"), Authorize(Roles = "Therapist,Doctor,Admin")]
        public async Task<IActionResult> EditCertificateAsync([FromRoute] string id, [FromForm] UploadImageDTO data)
        {
            if (!ModelState.IsValid)
                return BadRequest("Enter a valid image.");

            var certificate = await _context.certificates.FindAsync(id);
            if (certificate == null)
                return NotFound("Certificate not found.");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            // تحقق من الملكية أو الدور
            if (!isAdmin && certificate.TherapistAndDoctorId != currentUserId)
                return Unauthorized("You are not authorized to edit this certificate.");

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var extension = Path.GetExtension(data.Image.FileName)?.ToLowerInvariant();

            if (data.Image.Length > 5 * 1024 * 1024 || !allowedExtensions.Contains(extension))
                return BadRequest("Invalid file format or size.");

            try
            {
                var oldPath = Path.Combine("wwwroot", "Images_of_Certificate", Path.GetFileName(certificate.Path));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                string imageDirectory = Path.Combine("wwwroot", "Images_of_Certificate");
                if (!Directory.Exists(imageDirectory))
                    Directory.CreateDirectory(imageDirectory);

                string fileName = $"{currentUser.Name}_{Guid.NewGuid()}.jpg";
                string filePath = Path.Combine(imageDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await data.Image.CopyToAsync(stream);
                }

                certificate.Path = $"{_urlHelperService.GetCurrentServerUrl()}/Images_of_Certificate/{fileName}";
                await _context.SaveChangesAsync();

                return Ok("Certificate updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("deleteCertificate/{id}"), Authorize(Roles = "Therapist,Doctor,Admin")]
        public async Task<IActionResult> DeleteCertificateAsync([FromRoute] string id)
        {
            var certificate = await _context.certificates.FindAsync(id);
            if (certificate == null)
                return NotFound("Certificate not found.");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            // تحقق من الملكية أو الدور
            if (!isAdmin && certificate.TherapistAndDoctorId != currentUserId)
                return Unauthorized("You are not authorized to delete this certificate.");

            try
            {
                var imagePath = Path.Combine("wwwroot", "Images_of_Certificate", Path.GetFileName(certificate.Path));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);

                _context.certificates.Remove(certificate);
                await _context.SaveChangesAsync();

                return Ok("Certificate deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("getAllCertificateByUserId/{userId}"), Authorize]
        public async Task<IActionResult> GetAllCertificatesByUserId(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            // جلب كل الشهادات الخاصة بالمستخدم
            var certificates = await _context.certificates
                .Where(c => c.TherapistAndDoctorId == userId)
                .Include(c => c.TherapistAndDoctor)
                .ToListAsync();

            return Ok(certificates.Select(e => new
            {
                e.Path,
                e.Id,
                e.TherapistAndDoctor.Name
            }));
        }

        [HttpGet("getCertificateById/{id}"), Authorize]
        public async Task<IActionResult> GetCertificateById([FromRoute] int id)
        {
            var certificate = await _context.certificates.Include(e => e.TherapistAndDoctor).SingleOrDefaultAsync(e => e.Id == id);
            if (certificate == null)
                return NotFound("Certificate not found.");

            var result = new
            {
                certificate.Id,
                certificate.Path,
                certificate.TherapistAndDoctorId,
                certificate.TherapistAndDoctor.Name
            };

            return Ok(result);
        }

        [HttpPut("ApproveDoctor/{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveDoctor([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id) as TherapistAndDoctor;
            if (user == null)
                return NotFound("Doctor not found.");

            if (!user.IsApproved)
            {
                user.IsApproved = true;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            return Ok("Doctor approved successfully.");
        }

        [HttpDelete("RejectDoctor/{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectDoctor([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id) as TherapistAndDoctor;
            if (user == null)
                return NotFound("Doctor not found.");

            // Delete related certificates and files
            var certs = await _context.certificates
                .Where(c => c.TherapistAndDoctorId == id)
                .ToListAsync();
            foreach (var cert in certs)
            {
                var certPath = Path.Combine("wwwroot", "Images_of_Certificate", Path.GetFileName(cert.Path));
                if (System.IO.File.Exists(certPath))
                    System.IO.File.Delete(certPath);
            }
            _context.certificates.RemoveRange(certs);

            // Delete profile picture
            if (!string.IsNullOrEmpty(user.pathProfilePicture))
            {
                var picFile = Path.Combine("wwwroot", "Images_of_ProfilePicture", Path.GetFileName(user.pathProfilePicture));
                if (System.IO.File.Exists(picFile))
                    System.IO.File.Delete(picFile);
            }

            // Delete the user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));

            await _context.SaveChangesAsync();
            return Ok("Doctor registration rejected and user deleted.");
        }

        [HttpGet("GetNumberofAllDoctors")]
        public async Task<IActionResult> GetNumberofAllDoctors()
        {
            var numberOfDoctors = await _context.TherapistsAndDoctors.CountAsync(t => t.IsApproved);
            return Ok(new { NumberOfDoctors = numberOfDoctors });
        }

        [HttpGet("GetAllDoctorsOrTherapistsNotApproved")]
        public IActionResult GetNumberofDoctors() {
            return Ok(_context.TherapistsAndDoctors.Include(e=>e.Specialization).Where(t => !t.IsApproved).Select(t => new
            {
                t.Id,
                t.Name,
                t.Email,
                t.PhoneNumber,
                t.City,
                nameOfSpecialization = t.Specialization.Name,
                t.BornDate,
                t.DateJoined,
                t.pathProfilePicture,
                t.PathOfLicence
            }).ToList());
        }
    }
}
