using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystemApi.Data;
using SchoolManagementSystemApi.Models;
using SchoolManagementSystemApi.Services;

namespace SchoolManagementSystemApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("students/{classId}")]
        public async Task<IActionResult> GetStudents(int classId)
        {
            var students = await _context.Students
                .Where(s => s.Class.Id == classId)
                .ToListAsync();
            return Ok(students);
        }

        [HttpPost("mark")]
        public async Task<IActionResult> MarkAttendance([FromBody] List<Attendance> attendances)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Attendances.AddRange(attendances);
            await _context.SaveChangesAsync();

            var notificationService = HttpContext.RequestServices.GetService<NotificationService>();
            foreach (var attendance in attendances.Where(a => a.Status == "Absent"))
            {
                var student = await _context.Students.FindAsync(attendance.StudentId);
                if (student != null)
                {
                    notificationService.SendSms(student.ParentPhone, $"Your child {student.Name} was absent on {attendance.Date:yyyy-MM-dd}.");
                    notificationService.SendEmail(student.ParentEmail, "Absence Notification", $"Your child {student.Name} was absent on {attendance.Date:yyyy-MM-dd}.");
                }
            }
            return Ok(new { Message = "Attendance marked successfully" });
        }
    }
}