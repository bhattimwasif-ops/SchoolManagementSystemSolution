using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystemApi.Data;
using SchoolManagementSystemApi.Models;

namespace SchoolManagementSystemApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentTestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentTestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-marks")]
        public async Task<IActionResult> AddStudentMarks([FromBody] List<StudentTestDto> studentTests)
        {
            foreach (var st in studentTests)
            {
                var studentTest = new StudentTest
                {
                    StudentId = st.StudentId,
                    TestId = st.TestId,
                    Subject = st.Subject,
                    TotalMarks = st.TotalMarks,
                    ObtainedMarks = st.ObtainedMarks,
                    Percentage = (decimal)st.ObtainedMarks / st.TotalMarks * 100,
                    Grade = AssignGrade((decimal)st.ObtainedMarks / st.TotalMarks * 100)
                };
                _context.StudentTests.Add(studentTest);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetStudentReports()
        {
            var reports = await _context.StudentTests
                .Include(st => st.Test)
                .Select(st => new
                {
                    st.Id,
                    TestName = st.Test.Name,
                    st.Subject,
                    st.TotalMarks,
                    st.ObtainedMarks,
                    st.Percentage,
                    st.Grade,
                    st.UpdatedAt
                })
                .ToListAsync();
            return Ok(reports);
        }

        [HttpGet("{studentId}/reports")]
        public async Task<IActionResult> GetStudentReports(int studentId)
        {
            var reports = await _context.StudentTests
                .Where(st => st.StudentId == studentId)
                .Include(st => st.Test)
                .Select(st => new
                {
                    st.Id,
                    TestName = st.Test.Name,
                    st.Subject,
                    st.TotalMarks,
                    st.ObtainedMarks,
                    st.Percentage,
                    st.Grade,
                    st.UpdatedAt
                })
                .ToListAsync();
            return Ok(reports);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMarks(int id, [FromBody] StudentTestDto studentTestDto)
        {
            var studentTest = await _context.StudentTests.FindAsync(id);
            if (studentTest == null) return NotFound();

            var userId = User.Identity?.Name ?? "Unknown"; // Get user ID from JWT or fallback
            studentTest.Subject = studentTestDto.Subject;
            studentTest.TotalMarks = studentTestDto.TotalMarks;
            studentTest.ObtainedMarks = studentTestDto.ObtainedMarks;
            studentTest.Percentage = (decimal)studentTestDto.ObtainedMarks / studentTestDto.TotalMarks * 100;
            studentTest.Grade = AssignGrade(studentTest.Percentage);
            studentTest.UpdatedAt = DateTime.UtcNow;
            studentTest.UpdatedBy = userId;

            _context.StudentTests.Update(studentTest);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private string AssignGrade(decimal percentage)
        {
            return percentage >= 90 ? "A+" :
                   percentage >= 80 ? "A" :
                   percentage >= 70 ? "B" :
                   percentage >= 60 ? "C" :
                   percentage >= 50 ? "D" : "F";
        }
    }
}
