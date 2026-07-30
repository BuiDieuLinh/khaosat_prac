using khaosat_api.DTOs;
using khaosat_api.Models;
using khaosat_api.Repositories;
using khaosat_api.Repositories.Interfaces;
using khaosat_api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace khaosat_api.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IConfiguration _configuration;

        public EmployeeService(IEmployeeRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public AuthResponseDto? Login(EmployeeLoginDto dto)
        {
            Employee? employee = _repository.GetByEmail(dto.Username);
            if (employee == null)
            {
                employee = _repository.GetByEmployeeCode(dto.Username);
            }

            if (employee == null || !employee.IsActive)
            {
                return null;
            }

            if (!PasswordHasher.Verify(dto.Password, employee.PasswordHash))
            {
                return null;
            }

            string token = GenerateJwtToken(employee);

            return new AuthResponseDto
            {
                Id = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                FullName = employee.FullName,
                Email = employee.Email,
                Token = token
            };
        }

        public bool Register(EmployeeRegisterDto dto)
        {
            if (_repository.GetByEmployeeCode(dto.EmployeeCode) != null || _repository.GetByEmail(dto.Email) != null)
            {
                return false;
            }

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _repository.Add(employee);
            return true;
        }

        private string GenerateJwtToken(Employee employee)
        {
            var secretKey = _configuration["Jwt:Secret"] ?? "SuperSecretKeyMustBeAtLeast32BytesLong1234567890!";
            var issuer = _configuration["Jwt:Issuer"] ?? "khaosat_api";
            var audience = _configuration["Jwt:Audience"] ?? "khaosat_fe";
            var expiryInMinutes = Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"] ?? "180");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.FullName),
                new Claim(ClaimTypes.Email, employee.Email),
                new Claim(ClaimTypes.UserData, employee.EmployeeCode)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public List<EmployeeResponseDto> GetAll()
        {
            var employees = _repository.GetAll();
            return employees.Select(e => new EmployeeResponseDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Email = e.Email,
                IsActive = e.IsActive,
                CreatedDate = e.CreatedDate,

                PositionId = e.PositionId,
                PositionName = e.PositionName,
                PositionCode = e.PositionCode,

                DepartmentId = e.DepartmentId,
                DepartmentName = e.DepartmentName,
                DepartmentCode = e.DepartmentCode,

                Roles = e.Roles ?? new List<Role>(),
                RoleIds = e.RoleIds ?? new List<Guid>()
            }).ToList();
        }

        public List<Department> GetDepartment()
        {
            return _repository.GetDepartment();
        } 
        
        public List<Position> GetPosition(Guid departmentId)
        {
            return _repository.GetPosition(departmentId);
        }

        public List<Role> GetRoles()
        {
            return _repository.GetRoles();
        }

        public void Create(EmployeeCreateDto dto)
        {
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = PasswordHasher.Hash(dto.Password ?? "123456"),
                IsActive = dto.IsActive,
                PositionId = dto.PositionId,
                CreatedDate = DateTime.Now
            };

            _repository.Create(employee, dto.RoleIds);
        }

        public void Update(Guid id, EmployeeUpdateDto dto)
        {
            var employee = new Employee
            {
                Id = id,
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                Email = dto.Email,
                IsActive = dto.IsActive,
                PositionId = dto.PositionId
            };

            _repository.Update(employee, dto.RoleIds);
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }
    }
}
