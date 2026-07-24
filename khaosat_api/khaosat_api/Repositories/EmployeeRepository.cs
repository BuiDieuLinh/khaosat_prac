using khaosat_api.Data;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly SqlConnectionFactory _factory;

        public EmployeeRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public Employee? GetByEmployeeCode(string employeeCode)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT * FROM Employee WHERE EmployeeCode = @EmployeeCode";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public Employee? GetByEmail(string email)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT * FROM Employee WHERE Email = @Email";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public void Add(Employee employee)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO Employee
                (
                    Id,
                    EmployeeCode,
                    FullName,
                    Email,
                    PasswordHash,
                    IsActive,
                    CreatedDate
                )
                VALUES
                (
                    @Id,
                    @EmployeeCode,
                    @FullName,
                    @Email,
                    @PasswordHash,
                    @IsActive,
                    @CreatedDate
                )";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", employee.Id);
            cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
            cmd.Parameters.AddWithValue("@FullName", employee.FullName);
            cmd.Parameters.AddWithValue("@Email", employee.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", employee.PasswordHash);
            cmd.Parameters.AddWithValue("@IsActive", employee.IsActive);
            cmd.Parameters.AddWithValue("@CreatedDate", employee.CreatedDate);

            cmd.ExecuteNonQuery();
        }

        public int GetActiveEmployeeCount()
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT COUNT(*) FROM Employee WHERE IsActive = 1";
            using var cmd = new SqlCommand(sql, conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static Employee MapFromReader(SqlDataReader reader)
        {
            return new Employee
            {
                Id = Guid.Parse(reader["Id"].ToString()!),
                EmployeeCode = reader["EmployeeCode"].ToString()!,
                FullName = reader["FullName"].ToString()!,
                Email = reader["Email"].ToString()!,
                PasswordHash = reader["PasswordHash"].ToString()!,
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
            };
        }
    }
}
