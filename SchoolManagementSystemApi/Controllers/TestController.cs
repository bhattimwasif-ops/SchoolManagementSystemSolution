using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost("create")]
    public async Task<IActionResult> CreateTest([FromBody] TestDto testDto)
    {
        var test = new Test
        {
            Name = testDto.Name,
            ClassId = testDto.ClassId,
            Date = testDto.Date
        };
        _context.Tests.Add(test);
        await _context.SaveChangesAsync();
        return Ok(test.Id);
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
    public int ClassId { get; set; }
    public DateTime Date { get; set; }
}

public class StudentTestDto
{
    public int StudentId { get; set; }
    public int TestId { get; set; }
    public string Subject { get; set; } = null!;
    public int TotalMarks { get; set; }
    public int ObtainedMarks { get; set; }
}