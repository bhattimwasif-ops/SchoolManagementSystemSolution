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
                //.Select(s => new { s.Id, s.Name, s.ClassId })
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
                ParentPhone = studentDto.ParentPhone,
                GuardianOccupation = studentDto.GuardianOccupation,
                GuardianName = studentDto.GuardianName,
                Address = studentDto.Address,
                AdmissionDate = studentDto.AdmissionDate,
                RollNo = studentDto.RollNo,
                CreateBy = User?.Identity?.Name,
                Image = studentDto.Image,
                CreatedOn = DateTime.Now,
                FatherOccupation = studentDto.FatherOccupation,
                MotherName = studentDto.MotherName,                
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound("Student not found");

            return Ok(student);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentDto studentDto)
        {
            if (id != studentDto.Id)
                return BadRequest("Student ID mismatch");

            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound("Student not found");

            student.Name = studentDto.Name;
            student.ClassId = studentDto.ClassId;
            student.ParentEmail = studentDto.ParentEmail;
            student.ParentPhone = studentDto.ParentPhone;
            student.Image = studentDto.Image;
            student.Address = studentDto.Address;
            student.RollNo = studentDto.RollNo;
            student.FatherOccupation = studentDto.FatherOccupation;
            student.GuardianName = studentDto.GuardianName;
            student.GuardianOccupation = studentDto.GuardianOccupation;
            student.MotherName = studentDto.MotherName;
            student.AdmissionDate = studentDto.AdmissionDate;
            student.ModifiedBy = User?.Identity?.Name;
            student.ModifiedOn = DateTime.Now;

            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            return Ok("Student updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound("Student not found");

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Ok("Student deleted successfully");
        }


    }

    public class StudentDto
    {        
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ClassId { get; set; }
        public string ParentEmail { get; set; } = null!;
        public string ParentPhone { get; set; } = null!;
        public string Image { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string RollNo { get; set; } = null!;
        public string FatherOccupation { get; set; } = null!;
        public string GuardianName { get; set; } = null!;
        public string GuardianOccupation { get; set; } = null!;
        public string MotherName { get; set; } = null!;
        public DateTime? AdmissionDate { get; set; }
        public string? CreateBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
