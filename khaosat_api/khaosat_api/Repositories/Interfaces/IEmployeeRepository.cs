using khaosat_api.Models;
using System;

namespace khaosat_api.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Employee? GetByEmployeeCode(string employeeCode);
        Employee? GetByEmail(string email);
        void Add(Employee employee);
        int GetActiveEmployeeCount();
    }
}
