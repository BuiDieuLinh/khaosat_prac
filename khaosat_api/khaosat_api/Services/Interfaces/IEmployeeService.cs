using khaosat_api.DTOs;

namespace khaosat_api.Services.Interfaces
{
    public interface IEmployeeService
    {
        AuthResponseDto? Login(EmployeeLoginDto dto);
        bool Register(EmployeeRegisterDto dto);
    }
}
