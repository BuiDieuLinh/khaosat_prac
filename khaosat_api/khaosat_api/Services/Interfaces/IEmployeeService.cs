using khaosat_api.DTOs;
using khaosat_api.Models;
using System;
using System.Collections.Generic;

namespace khaosat_api.Services.Interfaces
{
    public interface IEmployeeService
    {
        AuthResponseDto? Login(EmployeeLoginDto dto);
        bool Register(EmployeeRegisterDto dto);

        List<EmployeeResponseDto> GetAll();
        List<Department> GetDepartment();
        List<Position> GetPosition(Guid departmentId);
        List<Role> GetRoles();
        void Create(EmployeeCreateDto dto);
        void Update(Guid id, EmployeeUpdateDto dto);
        void Delete(Guid id);
    }
}
