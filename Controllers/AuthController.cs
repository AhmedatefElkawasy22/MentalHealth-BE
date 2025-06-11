using MentalHealth_BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MentalHealth_BackEnd.DTO.Auth;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Azure;
using MentalHealth_BackEnd.helpers;

namespace MentalHealth_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IUrlHelperService _urlHelperService;
        private readonly JwtOptions _jwtOptions;

        public AuthController(UserManager<User> userManager, ApplicationDbContext context, IUrlHelperService urlHelperService, JwtOptions jwtOptions)
        {
            _userManager = userManager;
            _context = context;
            _urlHelperService = urlHelperService;
            _jwtOptions = jwtOptions;
        }

        [HttpPost("RegistrationAsVisitor")]
        public async Task<IActionResult> RegistrationAsVisitorAsync([FromForm] RegisterUserDTO data)
        {
            if (!ModelState.IsValid)
                return BadRequest("enter a valid Data.");

            if (await _userManager.Users.SingleOrDefaultAsync(u => u.Name == data.Name) is not null || await _userManager.FindByEmailAsync(data.Email) is not null)
                return BadRequest("This user already exists, please try another name or email.");

            #region add profile picture
            string profilePictureDirectory = Path.Combine("wwwroot", "Images_of_ProfilePicture");
            if (!Directory.Exists(profilePictureDirectory))
                Directory.CreateDirectory(profilePictureDirectory);

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var extensionOFprofilePicture = Path.GetExtension(data.ProfilePicture.FileName)?.ToLowerInvariant();

            if (data.ProfilePicture.Length > 5 * 1024 * 1024 || !allowedExtensions.Contains(extensionOFprofilePicture))
                return BadRequest("Invalid Profile Picture file format or size. Allowed: .png, .jpg, .jpeg under 5MB.");

            string fileNameOFprofilePicture = $"{data.Name}_{Guid.NewGuid()}.jpg";
            string filePathextensionOFprofilePicture = Path.Combine(profilePictureDirectory, fileNameOFprofilePicture);

            using (var stream = new FileStream(fileNameOFprofilePicture, FileMode.Create))
            {
                await data.ProfilePicture.CopyToAsync(stream);
            }

            string profilePictureUrl = $"{_urlHelperService.GetCurrentServerUrl()}/Images_of_ProfilePicture/{fileNameOFprofilePicture}";
            #endregion

            var identityUser = new Visitor
            {
                UserName = $"{data.Name.Split(" ")[0]}_{data.Email.Split("@")[0]}",
                Name = data.Name,
                Email = data.Email,
                Address = data.Address,
                PhoneNumber = data.PhoneNumber,
                BornDate = data.BornDate,
                gender = data.gender,
                pathProfilePicture = profilePictureUrl,
            };

            var result = await _userManager.CreateAsync(identityUser, data.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                    errors += $"{error.Description},";
                return BadRequest(errors);
            }

            //Add Visitor
            IdentityResult roleResult = await _userManager.AddToRoleAsync(identityUser, "Visitor");
            if (!roleResult.Succeeded)
            {
                // delete the user if adding to role failed
                await _userManager.DeleteAsync(identityUser);
                var errors = string.Empty;
                foreach (var error in roleResult.Errors)
                    errors += $"{error.Description},";

                return BadRequest(errors);
            }

            return Ok("registration successfully.");

        }

        [HttpPost("RegistrationAsTherapistOrDoctor")]
        public async Task<IActionResult> RegistrationAsTherapisOrDoctortAsync([FromForm] RegisterTherapistOrDoctorDTO data)
        {
            if (!ModelState.IsValid)
                return BadRequest("Enter valid data.");

            if (await _userManager.Users.SingleOrDefaultAsync(u => u.Name == data.Name) is not null ||
                await _userManager.FindByEmailAsync(data.Email) is not null)
                return BadRequest("This user already exists, please try another name or email.");

            #region add licence
            string licenceDirectory = Path.Combine("wwwroot", "Images_of_Licence");
            if (!Directory.Exists(licenceDirectory))
                Directory.CreateDirectory(licenceDirectory);

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var extension = Path.GetExtension(data.License.FileName)?.ToLowerInvariant();

            if (data.License.Length > 5 * 1024 * 1024 || !allowedExtensions.Contains(extension))
                return BadRequest("Invalid license file format or size. Allowed: .png, .jpg, .jpeg under 5MB.");

            string fileName = $"{data.Name}_{Guid.NewGuid()}.jpg";
            string filePath = Path.Combine(licenceDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await data.License.CopyToAsync(stream);
            }

            string licenceUrl = $"{_urlHelperService.GetCurrentServerUrl()}/Images_of_Licence/{fileName}";
            #endregion

            #region add profile picture
            string profilePictureDirectory = Path.Combine("wwwroot", "Images_of_ProfilePicture");
            if (!Directory.Exists(profilePictureDirectory))
                Directory.CreateDirectory(profilePictureDirectory);

            var extensionOFprofilePicture = Path.GetExtension(data.ProfilePicture.FileName)?.ToLowerInvariant();

            if (data.ProfilePicture.Length > 5 * 1024 * 1024 || !allowedExtensions.Contains(extensionOFprofilePicture))
                return BadRequest("Invalid Profile Picture file format or size. Allowed: .png, .jpg, .jpeg under 5MB.");

            string fileNameOFprofilePicture = $"{data.Name}_{Guid.NewGuid()}.jpg";
            string filePathextensionOFprofilePicture = Path.Combine(profilePictureDirectory, fileNameOFprofilePicture);

            using (var stream = new FileStream(filePathextensionOFprofilePicture, FileMode.Create))
            {
                await data.ProfilePicture.CopyToAsync(stream);
            }

            string profilePictureUrl = $"{_urlHelperService.GetCurrentServerUrl()}/Images_of_ProfilePicture/{fileNameOFprofilePicture}";
            #endregion

            var identityUser = new TherapistAndDoctor
            {
                UserName = $"{data.Name.Split(" ")[0]}_{data.Email.Split("@")[0]}",
                Name = data.Name,
                Email = data.Email,
                Address = data.Address,
                PhoneNumber = data.PhoneNumber,
                BornDate = data.BornDate,
                gender = data.gender,
                City = data.City,
                SpecializationID = data.SpecializationID,
                PathOfLicence = licenceUrl,
                pathProfilePicture= profilePictureUrl
            };

            var result = await _userManager.CreateAsync(identityUser, data.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }
            string role = data.role == Role.Therapist ? "Therapist" : "Doctor";
            var roleResult = await _userManager.AddToRoleAsync(identityUser, role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(identityUser);
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }

            return Ok("Registration successful.");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginUserDTO data)
        {
            var user = await _userManager.FindByEmailAsync(data.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, data.Password))
                return Unauthorized( "Email or password is incorrect." );

            if (user is TherapistAndDoctor v && !v.IsApproved ) 
            {
                return Unauthorized("You are not authorized to enter.");
            }

            var jwtSecurityToken = await CreateJwtTokenAsync(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            TherapistAndDoctor? therapist = user as TherapistAndDoctor;

            var authModel = new
            {
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpiresOn = jwtSecurityToken.ValidTo,
                Roles = rolesList.ToList(),
            };

            return Ok(authModel);
        }

        private async Task<JwtSecurityToken> CreateJwtTokenAsync(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim(ClaimTypes.Role, role));

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            }.Union(userClaims).Union(roleClaims);


            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwtOptions.LifeTime),
                signingCredentials: signingCredentials
                );

            return jwtSecurityToken;
        }



    }
}
