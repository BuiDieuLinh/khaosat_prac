using khaosat_api.Models;
using System;
using System.Collections.Generic;

namespace khaosat_api.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Employee? GetByEmployeeCode(string employeeCode);
        Employee? GetByEmail(string email);
        void Add(Employee employee);
        int GetActiveEmployeeCount();
        List<EmployeeResponse> GetAll();
        List<Department> GetDepartment();
        List<Position> GetPosition(Guid departmentId);
        List<Role> GetRoles();
        void Create(Employee employee, List<Guid> roleIds);
        void Update(Employee employee, List<Guid> roleIds);
        void Delete(Guid id);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> CheckEmployeeCodeExistsAsync(string employeeCode);
    }
}
