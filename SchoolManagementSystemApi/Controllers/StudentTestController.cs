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
        public async Task<IActionResult> GetStudentResult(int studentId, int? testId)
        {
            var studentTests = await _context.StudentTests
                .Where(st => st.StudentId == studentId && st.TestId == testId)
                .Include(st => st.Test)
                .Include(st => st.Student)
                .ToListAsync();

            if (!studentTests.Any())
            {
                return NotFound(new { message = "No result data found for this student." });
            }

            var subjects = studentTests.Select(st => new
            {
                st.Subject,
                st.TotalMarks,
                st.ObtainedMarks,
                Grade = GetGrade(st.ObtainedMarks, st.TotalMarks),
                Percentile = GetPercentile(st.ObtainedMarks, st.TotalMarks),
                Status = st.ObtainedMarks >= (st.TotalMarks * 0.50) ? "PASS" : "FAIL"
            }).ToList();

            var totalMarks = studentTests.Sum(st => st.TotalMarks);
            var obtainedMarks = studentTests.Sum(st => st.ObtainedMarks);
            var totalStatus = obtainedMarks >= (totalMarks * 0.50) ? "PASS" : "FAIL";

            var student = await _context.Students
                .Where(s => s.Id == studentId)
                .Select(s => new
                {
                    s.Name,
                    s.RollNo,
                    FatherName = s.GuardianName ?? "N/A", // Adjust model if needed
                    FatherCNIC = "N/A", // Adjust model if needed
                    DateOfBirth = Convert.ToString(s.DateOfBirth) ?? "N/A",
                    Institution = "NOMI PUBLIC HIGH SCHOOL" // Adjust model if needed
                })
                .FirstOrDefaultAsync();

            if (student == null) return NotFound();

            return Ok(new
            {
                name = student.Name,
                rollNo = student.RollNo,
                fatherName = student.FatherName,
                fatherCNIC = student.FatherCNIC,
                dateOfBirth = student.DateOfBirth,
                institution = "NOMI PUBLIC HIGH SCHOOL",
                subjects = subjects,
                totalMarks = totalMarks,
                totalStatus = $"{totalStatus} {obtainedMarks}/{totalMarks}",
                resultDeclaredOn = DateTime.Now.ToString("dd MMM yyyy")
            });
        }
        
        [HttpGet("{testId}/students")]
        public async Task<IActionResult> GetStudentsByTest(int testId)
        {
            var studentIds = await _context.StudentTests
            .Where(x => x.TestId == testId)
            .Select(x => x.StudentId)
            .Distinct()
            .ToListAsync();

            var students = await _context.Students
                .Where(x => studentIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();
            return Ok(students);
        }
        private string GetGrade(int obtained, int total)
        {
            var percentage = (obtained * 100) / total;
            return percentage >= 90 ? "A+" : percentage >= 80 ? "A" : percentage >= 70 ? "B" : percentage >= 60 ? "C" : "F";
        }

        private string GetPercentile(int obtained, int total)
        {
            var percentage = (obtained * 100) / total;
            return $"{percentage:0.00}";
        }
    }
}
