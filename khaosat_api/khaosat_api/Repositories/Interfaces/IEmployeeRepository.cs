using System;
using System.Collections.Generic;
using khaosat_api.DTOs;
using khaosat_api.Models;

namespace khaosat_api.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Employee? GetByEmployeeCode(string employeeCode);
        Employee? GetByEmail(string email);
        void Add(Employee employee);
        int GetActiveEmployeeCount();
        List<EmployeeResponse> GetAll();
        List<Guid> GetActiveAdministratorIds();
        PagedResult<EmployeeResponse> GetPaged(EmployeeFilterDto filter);
        List<Department> GetDepartment();
        List<Position> GetPosition(Guid departmentId);
        List<Role> GetRoles();
        List<Role> GetRolesByEmployeeId(Guid employeeId);
        void Create(Employee employee, List<Guid> roleIds);
        void Update(Employee employee, List<Guid> roleIds);
        void Delete(Guid id);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> CheckEmployeeCodeExistsAsync(string employeeCode);
        Task<EmployeeResponse?> GetByIdAsync(Guid guid);
    }
}
