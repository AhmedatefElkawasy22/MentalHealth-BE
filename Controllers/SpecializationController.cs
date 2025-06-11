using AutoMapper;
using MentalHealth_BackEnd.DTO.Specialization;
using MentalHealth_BackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecializationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;      
        private readonly IMapper _mapper;

        public SpecializationController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecialization()
        {
            var specs = await _context.Specializations.ToListAsync();

            var specsDTO = _mapper.Map<List<GetSpecializations>>(specs);
            return Ok(specsDTO);
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromForm]AddSpecialization addSpecialization)
        {
            var newspecialization = _mapper.Map<Specialization>(addSpecialization);
            _context.Specializations.Add(newspecialization);
            await _context.SaveChangesAsync();

            return Ok(newspecialization);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] UpdateSpecialization updateSpecialization)
        {
            var specialization = await _context.Specializations.FindAsync(id);
            if (specialization == null)
            {
                return NotFound(new { Message = "Specialization not found" });
            }

            _mapper.Map(updateSpecialization, specialization); // Map updated values to the existing entity
            _context.Specializations.Update(specialization);
            await _context.SaveChangesAsync();

            return Ok(specialization);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var specialization = await _context.Specializations.FindAsync(id);
            if (specialization == null)
            {
                return NotFound(new { Message = "Specialization not found" });
            }

            _context.Specializations.Remove(specialization);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Specialization deleted successfully" });
        }
    }
}
