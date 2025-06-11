using AutoMapper;
using MentalHealth_BackEnd.DTO.Appointment;
using MentalHealth_BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MentalHealth_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppointmentsController(ApplicationDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        // POST: api/Appointments
        [HttpPost]
        public async Task<IActionResult> BookAppointment([FromBody] AppointmentCreateDto appointmentDto)
        {
           string currentUserID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var therapist = await _context.TherapistsAndDoctors.FindAsync(appointmentDto.TherapistId);
            if (therapist == null) return NotFound(new { Message = "Therapist not found" });
            var appointment = new Appointment
            {
               UserId = currentUserID,
               TherapistId = appointmentDto.TherapistId,
               Status = 0, // Assuming 0 means "Pending" status
               Notes = appointmentDto.Notes,
               AppointmentDate = appointmentDto.AppointmentDate,
            };
           
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var readDto = _mapper.Map<AppointmentReadDto>(appointment);
            return CreatedAtAction(nameof(GetAppointment), new { id = readDto.AppointmentId }, readDto);
        }

        // GET: api/Appointments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.TherapistAndDoctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(new { Message = "Appointment not found" });

            return Ok(new
            {
                appointment.Id,
                appointment.AppointmentDate,
                appointment.Notes,
                Status = appointment.Status.ToString(),
                User = new
                {
                    appointment.User.Id,
                    appointment.User.Name,
                    appointment.User.Email,
                    appointment.User.PhoneNumber,
                },
                Therapist = new
                {
                    appointment.TherapistAndDoctor.Name,
                }
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.TherapistAndDoctor)
                .ToListAsync();

            var result = appointments.Select(appointment => new
            {
                appointment.Id,
                appointment.AppointmentDate,
                appointment.Notes,
                Status = appointment.Status.ToString(),
                User = new
                {
                    appointment.User.Id,
                    appointment.User.Name,
                    appointment.User.Email,
                    appointment.User.PhoneNumber,
                },
                Therapist = new
                {
                    appointment.TherapistAndDoctor.Name,
                }
            });

            return Ok(result);
        }


        // PUT: api/Appointments/{id}
        [HttpPut("{id}"), Authorize(Roles = "Therapist,Doctor")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentUpdateDto updateDto)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound(new { Message = "Appointment not found" });
            string currentUserID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (appointment.TherapistId != currentUserID)
                return BadRequest("You do not have permission");

            appointment.AppointmentDate = updateDto.AppointmentDate;
            appointment.Notes = updateDto.Notes;
            appointment.Status = updateDto.Status; // Assuming Status is an enum or int
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            return Ok("Appointment Edit successfully.");
        }

        // DELETE: api/Appointments/{id}
        [HttpDelete("{id}"), Authorize(Roles = "Therapist,Doctor,Admin")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound(new { Message = "Appointment not found" });

            string currentUserID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (appointment.TherapistId != currentUserID)
                return BadRequest("You do not have permission");

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Appointment canceled successfully" });
        }

        [HttpGet("GetNumberOfAllUpcomingAppointments")]
        public async Task<IActionResult> GetNumberOfAllUpcomingAppointments()
        {

            var today = DateTime.Today;

            var count = await _context.Appointments
                .Where(a => a.AppointmentDate.Date >= today)
                .CountAsync();

            return Ok(new { UpcomingCount = count });
        }

        [HttpGet("therapist/{GetAllAppointmentsByTherapistOrDoctorId}")]
        [Authorize(Roles = "Therapist,Doctor")]
        public async Task<IActionResult> GetAllAppointmentsByTherapistOrDoctorId(string therapistId)
        {
            string currentUserID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (therapistId != currentUserID)
                return BadRequest("You do not have permission");
            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.TherapistAndDoctor)
                .Where(a => a.TherapistId == therapistId)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            var result = appointments.Select(appointment => new
            {
                appointment.Id,
                appointment.AppointmentDate,
                appointment.Notes,
                Status = appointment.Status.ToString(),
                User = new
                {
                    appointment.User.Id,
                    appointment.User.Name,
                    appointment.User.Email,
                    appointment.User.PhoneNumber,
                },
                Therapist = new
                {
                    appointment.TherapistAndDoctor.Id,
                    appointment.TherapistAndDoctor.Name,
                }
            });

            return Ok(result);
        }


        [HttpGet("GetAppointmentsOfCurrentUser")]
        public async Task<IActionResult> GetAppointmentsOfCurrentUser()
        {

            string currentUserID = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.TherapistAndDoctor)
                .Where(a => a.UserId == currentUserID)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            var result = appointments.Select(appointment => new
            {
                appointment.Id,
                appointment.AppointmentDate,
                appointment.Notes,
                Status = appointment.Status.ToString(),
                Therapist = new
                {
                    appointment.TherapistAndDoctor.Id,
                    appointment.TherapistAndDoctor.Name,
                    appointment.TherapistAndDoctor.Specialization
                }
            });

            return Ok(result);
        }

    }
}
