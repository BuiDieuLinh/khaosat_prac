using Microsoft.AspNetCore.Mvc;
using khaosat_api.DTOs;
using khaosat_api.Services.Interfaces;

namespace khaosat_api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public AuthController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] EmployeeLoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest(new { message = "Username and Password are required." });
            }

            var response = _employeeService.Login(dto);
            if (response == null)
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
            }

            return Ok(response);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] EmployeeRegisterDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.EmployeeCode) || string.IsNullOrEmpty(dto.FullName) ||
                string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ thông tin đăng ký." });
            }

            var result = _employeeService.Register(dto);
            if (!result)
            {
                return BadRequest(new { message = "Mã nhân viên hoặc Email đã tồn tại trong hệ thống." });
            }

            return Ok(new { message = "Đăng ký tài khoản nhân viên thành công." });
        }
    }
}
