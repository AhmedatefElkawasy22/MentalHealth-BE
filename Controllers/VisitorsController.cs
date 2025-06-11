using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MentalHealth.DTOs;
using MentalHealth_BackEnd.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace MentalHealth_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<Visitor> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public VisitorController(ApplicationDbContext context, IMapper mapper, UserManager<Visitor> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVisitors()
        {
 
                var role = await _roleManager.FindByNameAsync("Visitor");
                if (role == null)
                    return NotFound($"Role Visitor not found.");

                var usersInRole = await _userManager.GetUsersInRoleAsync("Visitor");

                var result = usersInRole.Select(user => new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.PhoneNumber,
                    user.BornDate,
                    user.Address,
                    user.gender,
                });

                return Ok(result);
            
        }

        // Get user by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var visitor = await _context.visitors.FirstOrDefaultAsync(u => u.Id == id);

            if (visitor == null)
                return NotFound();

            return Ok(new {visitor.Name , visitor.BornDate , visitor.Email , visitor.gender , visitor.PhoneNumber , visitor.Address });
        }
       
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromForm] UpdateVistorDTO updateVisitor)
        {
            var visitor = await _context.visitors.SingleOrDefaultAsync(v=>v.Id ==id);
            if (visitor == null)
            {
                return NotFound(new { Message = "visitor not found" });
            }

            _mapper.Map(updateVisitor, visitor); // Map updated values to the existing entity
            _context.visitors.Update(visitor);
            await _context.SaveChangesAsync();

            return Ok(visitor);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var visitor = await _context.visitors.SingleOrDefaultAsync(v => v.Id == id);
            if (visitor == null)
            {
                return NotFound(new { Message = "visitor not found" });
            }

            _context.visitors.Remove(visitor);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "visitor deleted successfully" });
        }

    }
}
