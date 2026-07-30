using khaosat_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace khaosat_api.Controllers
{
    [ApiController]
    [Route("api/department")]
    public class DepartmentController : Controller
    {
        private readonly IEmployeeService _departmentService;

        public DepartmentController(IEmployeeService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public IActionResult GetDepartments()
        {
            var departments = _departmentService.GetDepartment();
            return Ok(departments);
        }

        [HttpGet]
        [Route("positions")]
        public IActionResult GetPositions([FromQuery] Guid? departmentId)
        {
            var positions = _departmentService.GetPosition(departmentId ?? Guid.Empty);
            return Ok(positions);
        }
    }
}
