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
        var userId = User.Identity?.Name ?? "Unknown";

        var existingTests = _context.StudentTests
            .Where(x => studentTests.Select(st => st.StudentId).Contains(x.StudentId)
                     && studentTests.Select(st => st.TestId).Contains(x.TestId)
                     && studentTests.Select(st => st.Subject).Contains(x.Subject))
            .ToList();

        var skippedSubjects = new List<string>();

        foreach (var st in studentTests)
        {
            bool alreadyExists = existingTests.Any(x =>
                x.StudentId == st.StudentId &&
                x.TestId == st.TestId &&
                x.Subject == st.Subject);

            if (alreadyExists)
            {
                skippedSubjects.Add(st.Subject);
                continue;
            }

            var percentage = (decimal)st.ObtainedMarks / st.TotalMarks * 100;
            var studentTest = new StudentTest
            {
                StudentId = st.StudentId,
                TestId = st.TestId,
                Subject = st.Subject,
                TotalMarks = st.TotalMarks,
                ObtainedMarks = st.ObtainedMarks,
                Percentage = percentage,
                Grade = AssignGrade(percentage),
                UpdatedBy = userId,
                UpdatedAt = DateTime.Now
            };

            _context.StudentTests.Add(studentTest);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Marks processed.",
            SkippedSubjects = skippedSubjects
        });
    }

    [HttpPost("add-marks-row")]
    public async Task<IActionResult> AddStudentMarksByRow([FromBody] StudentTestDto studentTests)
    {
        var userId = User.Identity?.Name ?? "Unknown";

        var existingTests = _context.StudentTests
            .Where(x => studentTests.StudentId == x.StudentId//Select(st => st.StudentId).Contains(x.StudentId)
                     && studentTests.TestId == x.TestId
                     && studentTests.Subject.Contains(x.Subject))
            .ToList();

        var skippedSubjects = new List<string>();

        //foreach (var st in studentTests)
        //{
        bool alreadyExists = existingTests.Any(x =>
            x.StudentId == studentTests.StudentId &&
            x.TestId == studentTests.TestId &&
            x.Subject == studentTests.Subject);

        if (alreadyExists)
        {
            skippedSubjects.Add(studentTests.Subject);
            return Ok(new
            {
                Message = "Marks not added.",
                SkippedSubjects = skippedSubjects
            });
        }

        var percentage = (decimal)studentTests.ObtainedMarks / studentTests.TotalMarks * 100;
        var studentTest = new StudentTest
        {
            StudentId = studentTests.StudentId,
            TestId = studentTests.TestId,
            Subject = studentTests.Subject,
            TotalMarks = studentTests.TotalMarks,
            ObtainedMarks = studentTests.ObtainedMarks,
            Percentage = percentage,
            Grade = AssignGrade(percentage),
            UpdatedBy = userId,
            UpdatedAt = DateTime.Now
        };

        _context.StudentTests.Add(studentTest);
        //}

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Marks added.",
            SkippedSubjects = skippedSubjects
        });
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
    [HttpGet("{testId}/results")]
    public async Task<IActionResult> GetTestResults(int testId)
    {
        var studentTests = await _context.StudentTests
            .Where(st => st.TestId == testId)
            .Include(st => st.Test)
            .Include(st => st.Student)
            .ToListAsync();

        if (!studentTests.Any())
        {
            return NotFound(new { message = "No results found for this test." });
        }

        var results = studentTests
            .GroupBy(st => st.StudentId)
            .Select(g => new
            {
                studentId = g.Key,
                student = g.First().Student,
                subjects = g.Select(st => new
                {
                    subject = st.Subject,
                    totalMarks = st.TotalMarks,
                    obtainedMarks = st.ObtainedMarks,
                    grade = GetGrade(st.ObtainedMarks, st.TotalMarks),
                    percentile = GetPercentile(st.ObtainedMarks, st.TotalMarks),
                    status = st.ObtainedMarks >= (st.TotalMarks * 0.50) ? "PASS" : "FAIL"
                }).ToList(),
                totalMarks = g.Sum(st => st.TotalMarks),
                obtainedMarks = g.Sum(st => st.ObtainedMarks), // Preserve obtainedMarks
                totalStatus = g.Sum(st => st.ObtainedMarks) >= (g.Sum(st => st.TotalMarks) * 0.50) ? "PASS" : "FAIL"
            })
            .Select(r => new
            {
                studentId = r.studentId,
                name = r.student.Name,
                rollNo = r.student.RollNo,
                fatherName = r.student.GuardianName ?? "N/A",
                fatherRollNo = "N/A",
                dateOfBirth = r.student.DateOfBirth?.ToString("dd/MM/yyyy") ?? "N/A",
                institution = "N/A",
                subjects = r.subjects,
                totalMarks = r.totalMarks,
                obtainedMarks = r.obtainedMarks, // Include obtainedMarks in the final object
                totalStatus = $"{r.totalStatus} {r.obtainedMarks}/{r.totalMarks}",
                resultDeclaredOn = DateTime.Now.ToString("dd MMM yyyy")
            })
            .OrderByDescending(r => r.obtainedMarks) // Sort by obtainedMarks
            .ToList();

        return Ok(results);
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