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

        [HttpGet("{classId}/marks")]
        public async Task<IActionResult> GetMarksForClass(int classId, int? testId, int? studentId)
        {
            var query = _context.StudentTests.Include(st => st.Test).Include(st => st.Student).AsQueryable();
            if (testId.HasValue) query = query.Where(st => st.TestId == testId.Value);
            if (studentId.HasValue) query = query.Where(st => st.StudentId == studentId.Value);
            var results = await query.Select(st => new
            {
                st.Id,
                TestName = st.Test.Name,
                StudentName = st.Student.Name,
                st.Subject,
                st.TotalMarks,
                st.ObtainedMarks,
                st.Percentage,
                st.Grade
            }).ToListAsync();
            return Ok(results);
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarks(int id)
        {
            var studentTest = await _context.StudentTests.FindAsync(id);
            if (studentTest == null) return NotFound();

            _context.StudentTests.Remove(studentTest);
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

        [HttpGet("{studentId}/result")]
        public async Task<IActionResult> GetStudentResult(int studentId)
        {
            var studentTests = await _context.StudentTests
                .Where(st => st.StudentId == studentId)
                .Include(st => st.Test)
                .Include(st => st.Student)
                .ToListAsync();

            if (!studentTests.Any())
            {
                return NotFound(new { message = "No result data found for this student." });
            }

            var subjects = studentTests.Select(st => new[]
            {
                //st.Test.Subject,
                //st.Test.TotalMarks,
                st.ObtainedMarks,
                //GetGrade(st.ObtainedMarks, st.Test.TotalMarks),
                //GetPercentile(st.ObtainedMarks, st.Test.TotalMarks),
                //st.ObtainedMarks >= (st.Test.TotalMarks * 0.33) ? "PASS" : "FAIL"
            }).ToList();

            var totalMarks = "490";//studentTests.Sum(st => st.Test.);
            var obtainedMarks = studentTests.Sum(st => st.ObtainedMarks);
            var totalStatus = "Pass";//obtainedMarks >= (totalMarks * 0.33) ? "PASS" : "FAIL";

            var student = await _context.Students
                .Where(s => s.Id == studentId)
                .Select(s => new
                {
                    s.Name,
                    s.RollNo,
                    FatherName = s.GuardianName ?? "N/A", // Adjust model if needed
                    //FatherRollNo = s.FatherRollNo ?? "N/A", // Adjust model if needed
                    //DateOfBirth = s.DateOfBirth?.ToString("dd/MM/yyyy") ?? "N/A",
                    //Institution = s.Class?.InstitutionName ?? "N/A" // Adjust model if needed
                })
                .FirstOrDefaultAsync();

            if (student == null) return NotFound();

            return Ok(new
            {
                name = student.Name,
                rollNo = student.RollNo,
                fatherName = student.FatherName,
                //fatherRollNo = student.FatherRollNo,
                //dateOfBirth = student.DateOfBirth,
                //institution = student.Institution,
                subjects = subjects,
                totalMarks = totalMarks,
                totalStatus = $"{totalStatus} {obtainedMarks}/{totalMarks}",
                resultDeclaredOn = DateTime.Now.ToString("dd MMM yyyy")
            });
        }
    }
}
