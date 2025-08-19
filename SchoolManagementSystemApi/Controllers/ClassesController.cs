using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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
    public class ClassesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses()
        {
            var classes = await _context.Classes.Select(x=> new {x.Id, x.ClassName, x.Section}).ToListAsync();
            return Ok(classes);
        }
        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] Class classDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var @class = new Class
            {
                ClassName = classDto.ClassName,
                Section = classDto.Section,
                Teacher = classDto.Teacher
            };

            _context.Classes.Add(@class);
            await _context.SaveChangesAsync();

            return Ok();
        }
      
    }
    public class ClassDto
    {        
        public int Id { get; set; }
        public string ClassName { get; set; } = null!;
        public string Section { get; set; } = null!;
        public string Teacher { get; set; } = null!;
    }
}
