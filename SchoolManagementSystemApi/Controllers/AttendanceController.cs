using System.Text.Json;
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

        [HttpPost("manual-mark")]
        public async Task<IActionResult> MarkAttendance([FromBody] IEnumerable<AttendanceDto> attendances)
        {
            if (attendances == null || !attendances.Any())
                return BadRequest("No attendance data provided.");

            try
            {
                var normalizedAttendances = attendances
                    .Select(a => new Attendance
                    {
                        StudentId = a.StudentId,
                        Date = DateTime.Parse(a.Date, null, System.Globalization.DateTimeStyles.RoundtripKind).Date,
                        Status = a.Status
                    })
                    .ToList();

                var studentIds = normalizedAttendances.Select(a => a.StudentId).Distinct().ToList();
                var dates = normalizedAttendances.Select(a => a.Date).Distinct().ToList();

                // Delete existing records for same students and dates
                var existingRecords = await _context.Attendances
                    .Where(a => studentIds.Contains(a.StudentId) && dates.Contains(a.Date))
                    .ToListAsync();

                _context.Attendances.RemoveRange(existingRecords);

                // Add new records
                _context.Attendances.AddRange(normalizedAttendances);

                await _context.SaveChangesAsync();
                return Ok("Attendance updated successfully.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing attendance.");
            }
        }

        //[HttpGet("class/{classId}")]
        //public async Task<IActionResult> GetClassAttendance(int classId)
        //{
        //    try
        //    {
        //        var attendanceData = await _context.Attendances
        //        .Where(a => a.Student.ClassId == classId)
        //        .Include(a => a.Student)
        //        .ThenInclude(s => s.Class)
        //        .ToListAsync();

        //        if (attendanceData == null || !attendanceData.Any())
        //        {
        //            return NotFound(new { message = "No attendance data found for this class." });
        //        }

        //        // Perform grouping and projection client-side
        //        var latestAttendance = attendanceData
        //            .GroupBy(a => a.StudentId)
        //            .Select(g => g.OrderByDescending(a => a.Date).First())
        //            .Select(a => new
        //            {
        //                Id = a.Student.Id,
        //                Name = a.Student.Name,
        //                //RollNumber = a.Student.RollNumber,
        //                Status = a.Status,
        //                Date = a.Date
        //            })
        //            .ToList();

        //        return Ok(latestAttendance);

        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetClassAttendance(int classId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Attendances
                .Where(a => a.Student.ClassId == classId);

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Date >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(a => a.Date <= toDate.Value);
            }

            var attendanceData = await query
                .Include(a => a.Student)
                .ThenInclude(s => s.Class)
                .ToListAsync();

            if (attendanceData == null || !attendanceData.Any())
            {
                return NotFound(new { message = "No attendance data found for this class." });
            }

            // Group by student ID and get the latest status per student
            var latestAttendance = attendanceData
                .GroupBy(a => a.StudentId)
                .Select(g => g.OrderByDescending(a => a.Date).First())
                .Select(a => new
                {
                    Id = a.Student.Id,
                    Name = a.Student.Name,
                    RollNumber = a.Student.RollNo,
                    Status = a.Status,
                    Date = a.Date
                })
                .ToList();

            // Calculate totals
            var totalPresent = latestAttendance.Count(a => a.Status == "Present");
            var totalAbsent = latestAttendance.Count(a => a.Status == "Absent");
            var totalLate = latestAttendance.Count(a => a.Status == "Late");
            var absentStudents = latestAttendance.Where(a => a.Status == "Absent").Select(a => a.Name).ToList();

            return Ok(new
            {
                Attendance = latestAttendance,
                Totals = new
                {
                    Present = totalPresent,
                    Absent = totalAbsent,
                    Late = totalLate
                },
                AbsentStudents = absentStudents
            });
        }
    }
    public class AttendanceDto
    {
        public int StudentId { get; set; }
        public string Date { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}