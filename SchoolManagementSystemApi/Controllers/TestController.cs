using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystemApi.Data;
using SchoolManagementSystemApi.Models;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("all-tests")]
    public async Task<IActionResult> GetAllTests()
    {
        var tests = await _context.Tests
            .Select(t => new { t.Id, t.Name, t.Type })
            .ToListAsync();
        return Ok(tests);
    }

    [HttpGet("{testId}/marks")]
    public async Task<IActionResult> GetMarksForTest(int testId, int? studentId)
    {
        var query = _context.StudentTests.Where(st => st.TestId == testId);
        if (studentId.HasValue) query = query.Where(st => st.StudentId == studentId.Value);
        var marks = await query.Select(st => new { st.Id, st.Student.Name, st.Subject, st.ObtainedMarks, st.TotalMarks, st.Percentage, st.Grade }).ToListAsync();
        return Ok(marks);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTest([FromBody] TestDto testDto)
    {
        var test = new Test
        {
            Name = testDto.Name,
            Type = testDto.Type,
            ClassId = testDto.ClassId,
            Session = testDto.Session,
            FromDate = testDto.FromDate,
            ToDate = testDto.ToDate,
            Date = testDto.Date // Optional, can be removed
        };
        _context.Tests.Add(test);
        await _context.SaveChangesAsync();
        return Ok(test.Id);
    }

    [HttpGet("{classId}/tests")]
    public async Task<IActionResult> GetTestsForClass(int classId)
    {
        var tests = await _context.Tests
            .Where(t => t.ClassId == classId)
            .Select(t => new { t.Id, t.Name, t.Type, t.Session, t.FromDate, t.ToDate })
            .ToListAsync();
        return Ok(tests);
    }
    [HttpPost("add-marks")]
    public async Task<IActionResult> AddStudentMarks([FromBody] List<StudentTestDto> studentTests)
    {
        var userId = User.Identity?.Name ?? "Unknown"; // Get user ID from JWT or fallback
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
                Grade = AssignGrade((decimal)st.ObtainedMarks / st.TotalMarks * 100),
                UpdatedBy = userId

            };
            _context.StudentTests.Add(studentTest);
        }
        await _context.SaveChangesAsync();
        return Ok();
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
}

public class TestDto
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = "Test";
    public int ClassId { get; set; }
    public string Session { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime Date { get; set; } // Optional, can be removed
}
public class StudentTestDto
{
    public int StudentId { get; set; }
    public int TestId { get; set; }
    public string Subject { get; set; } = null!;
    public int TotalMarks { get; set; }
    public int ObtainedMarks { get; set; }
}