using khaosat_api.DTOs;
using khaosat_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace khaosat_api.Controllers
{
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

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _employeeService.GetRoles();
            return Ok(roles);
        }

        [HttpPost]
        public IActionResult Create([FromBody] EmployeeCreateDto dto)
        {
            _employeeService.Create(dto);
            return Ok(new { success = true, message = "Thêm nhân sự thành công" });
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
    }
}
