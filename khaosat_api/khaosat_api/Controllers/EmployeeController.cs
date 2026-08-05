using khaosat_api.DTOs;
using khaosat_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace khaosat_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var employees = _employeeService.GetAll();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var employees = await _employeeService.GetByIdAsync(id);

            return Ok(employees);
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _employeeService.GetRoles();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _employeeService.Create(dto);
                return Ok(new { success = true, message = "Thêm nhân sự thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] EmployeeUpdateDto dto)
        {
            _employeeService.Update(id, dto);
            return Ok(new { success = true, message = "Cập nhật nhân sự thành công" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _employeeService.Delete(id);
            return Ok(new { success = true, message = "Xóa nhân sự thành công" });
        }

        [HttpGet("check-email-exists")]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email không được để trống.");
            }

            bool exists = await _employeeService.CheckEmailExistsAsync(email);

            return Ok(exists);
        }

        [HttpGet("check-employeeCode-exists")]
        public async Task<IActionResult> CheckEmployeeCodeExists(string employeeCode)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                return BadRequest("EmployeeCode không được để trống.");
            }

            bool exists = await _employeeService.CheckEmployeeCodeExistsAsync(employeeCode);

            return Ok(exists);
        }
    }
}
