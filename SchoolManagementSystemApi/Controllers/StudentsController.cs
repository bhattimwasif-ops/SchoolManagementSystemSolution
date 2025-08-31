using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystemApi.Data;
using SchoolManagementSystemApi.Models;

namespace SchoolManagementSystemApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Students
                .Select(s => new { s.Id, s.Name, s.ClassId })
                .ToListAsync();
            return Ok(students);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] StudentDto studentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _context.Classes.AnyAsync(c => c.Id == studentDto.ClassId))
            {
                return BadRequest("Invalid ClassId");
            }

            var student = new Student
            {
                Name = studentDto.Name,
                ClassId = studentDto.ClassId,
                ParentEmail = studentDto.ParentEmail,
                ParentPhone = studentDto.ParentPhone
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{classId}/students")]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            var students = await _context.Students
                .Where(s => s.ClassId == classId)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            return Ok(students);
        }        

    }

    public class StudentDto
    {        
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ClassId { get; set; }
        public string ParentEmail { get; set; } = null!;
        public string ParentPhone { get; set; } = null!;
    }
}
