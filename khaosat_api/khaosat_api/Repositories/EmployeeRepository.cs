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
                    CreatedDate,
                    PermissionVersion
                )
                VALUES
                (
                    @Id,
                    @EmployeeCode,
                    @FullName,
                    @Email,
                    @PasswordHash,
                    @IsActive,
                    @CreatedDate,
                    @PermissionVersion
                )";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", employee.Id);
            cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
            cmd.Parameters.AddWithValue("@FullName", employee.FullName);
            cmd.Parameters.AddWithValue("@Email", employee.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", employee.PasswordHash);
            cmd.Parameters.AddWithValue("@IsActive", employee.IsActive);
            cmd.Parameters.AddWithValue("@CreatedDate", employee.CreatedDate);
            cmd.Parameters.AddWithValue("@PermissionVersion", employee.PermissionVersion ?? Guid.NewGuid());

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

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
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
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                PermissionVersion = Guid.Parse(reader["PermissionVersion"].ToString()!),
            };
        }

        public List<EmployeeResponse> GetAll()
        {
            var list = new List<EmployeeResponse>();
            using var conn = _factory.Create();
            conn.Open();
            const string sql = @"
               SELECT
                        e.Id,
                        e.EmployeeCode,
                        e.FullName,
                        e.Email,
                        e.IsActive,
                        e.CreatedDate,

                        p.Id AS PositionId,
                        p.PositionName,
                        p.PositionCode,

                        d.Id AS DepartmentId,
                        d.DepartmentName,
                        d.DepartmentCode,

                        r.Id AS RoleId,
                        r.RoleName
                    FROM Employee e
                    LEFT JOIN Position p
                        ON e.PositionId = p.Id
                    LEFT JOIN Department d
                        ON p.DepartmentId = d.Id
                    LEFT JOIN UserRole er
                        ON e.Id = er.EmployeeId
                    LEFT JOIN Role r
                        ON er.RoleId = r.Id";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            var employees = new Dictionary<Guid, EmployeeResponse>();

            while (reader.Read())
            {
                var employeeId = (Guid)reader["Id"];
                if (!employees.TryGetValue(employeeId, out var employee))
                {
                    employee = new EmployeeResponse
                    {
                        Id = employeeId,
                        EmployeeCode = reader["EmployeeCode"].ToString()!,
                        FullName = reader["FullName"].ToString()!,
                        Email = reader["Email"].ToString()!,
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                        PositionId = reader["PositionId"] != DBNull.Value
                            ? (Guid)reader["PositionId"]
                            : Guid.Empty,
                        PositionCode = reader["PositionCode"]?.ToString() ?? "",
                        PositionName = reader["PositionName"]?.ToString() ?? "",
                        DepartmentId = reader["DepartmentId"] != DBNull.Value
                            ? (Guid)reader["DepartmentId"]
                            : null,
                        DepartmentCode = reader["DepartmentCode"]?.ToString() ?? "",
                        DepartmentName = reader["DepartmentName"]?.ToString() ?? "",
                        Roles = new List<Role>()
                    };
                    employees.Add(employeeId, employee);
                }

                if (reader["RoleId"] != DBNull.Value)
                {
                    employee.Roles.Add(new Role
                    {
                        Id = (Guid)reader["RoleId"],
                        RoleName = reader["RoleName"].ToString()!
                    });
                }
                
            }

            return employees.Values.ToList();
        }

        public async Task<EmployeeResponse?> GetByIdAsync(Guid id)
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();

            const string sql = @"
                SELECT
                    e.Id,
                    e.EmployeeCode,
                    e.FullName,
                    e.Email,
                    e.IsActive,
                    e.CreatedDate,
                    e.PermissionVersion,

                    p.Id AS PositionId,
                    p.PositionName,
                    p.PositionCode,

                    d.Id AS DepartmentId,
                    d.DepartmentName,
                    d.DepartmentCode,

                    r.Id AS RoleId,
                    r.RoleName
                FROM Employee e
                LEFT JOIN Position p
                    ON e.PositionId = p.Id
                LEFT JOIN Department d
                    ON p.DepartmentId = d.Id
                LEFT JOIN UserRole ur
                    ON e.Id = ur.EmployeeId
                LEFT JOIN Role r
                    ON ur.RoleId = r.Id
                WHERE e.Id = @Id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            EmployeeResponse? employee = null;

            while (await reader.ReadAsync())
            {
                if (employee == null)
                {
                    employee = new EmployeeResponse
                    {
                        Id = (Guid)reader["Id"],
                        EmployeeCode = reader["EmployeeCode"].ToString()!,
                        FullName = reader["FullName"].ToString()!,
                        Email = reader["Email"].ToString()!,
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),

                        PositionId = reader["PositionId"] != DBNull.Value
                            ? (Guid)reader["PositionId"]
                            : Guid.Empty,

                        PositionCode = reader["PositionCode"]?.ToString() ?? "",
                        PositionName = reader["PositionName"]?.ToString() ?? "",

                        DepartmentId = reader["DepartmentId"] != DBNull.Value
                            ? (Guid)reader["DepartmentId"]
                            : null,

                        DepartmentCode = reader["DepartmentCode"]?.ToString() ?? "",
                        DepartmentName = reader["DepartmentName"]?.ToString() ?? "",

                        // nếu có cột này
                        PermissionVersion = reader["PermissionVersion"] != DBNull.Value
                            ? (Guid)reader["PermissionVersion"]
                            : Guid.Empty,

                        Roles = new List<Role>()
                    };
                }

                if (reader["RoleId"] != DBNull.Value)
                {
                    employee.Roles.Add(new Role
                    {
                        Id = (Guid)reader["RoleId"],
                        RoleName = reader["RoleName"].ToString()!
                    });
                }
            }

            return employee;
        }
        public List<Department> GetDepartment()
        {
            var list = new List<Department>();
            using var conn = _factory.Create();
            conn.Open();
            try
            {
                const string sql = "SELECT * FROM Department";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Department
                    {
                        Id = HasColumn(reader, "Id") ? Guid.Parse(reader["Id"].ToString()!) : (HasColumn(reader, "DepartmentId") ? Guid.Parse(reader["DepartmentId"].ToString()!) : Guid.NewGuid()),
                        DepartmentName = HasColumn(reader, "DepartmentName") ? reader["DepartmentName"].ToString()! : (HasColumn(reader, "Name") ? reader["Name"].ToString()! : ""),
                        DepartmentCode = HasColumn(reader, "DepartmentCode") ? reader["DepartmentCode"].ToString()! : (HasColumn(reader, "Code") ? reader["Code"].ToString()! : ""),
                        Description = HasColumn(reader, "Description") ? reader["Description"].ToString()! : ""
                    });
                }
            }
            catch
            {
            }
            return list;
        }

        public List<Position> GetPosition(Guid departmentId)
        {
            var list = new List<Position>();
            using var conn = _factory.Create();
            conn.Open();
            try
            {
                const string sql = "SELECT * FROM Position WHERE DepartmentId = @DepartmentId OR @DepartmentId = '00000000-0000-0000-0000-000000000000'";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DepartmentId", departmentId);     
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Position
                    {
                        Id = HasColumn(reader, "Id") ? Guid.Parse(reader["Id"].ToString()!) : (HasColumn(reader, "PositionId") ? Guid.Parse(reader["PositionId"].ToString()!) : Guid.NewGuid()),
                        PositionName = HasColumn(reader, "PositionName") ? reader["PositionName"].ToString()! : (HasColumn(reader, "Name") ? reader["Name"].ToString()! : ""),
                        PositionCode = HasColumn(reader, "PositionCode") ? reader["PositionCode"].ToString()! : (HasColumn(reader, "Code") ? reader["Code"].ToString()! : ""),
                        Description = HasColumn(reader, "Description") ? reader["Description"].ToString()! : "",
                        DepartmentId = HasColumn(reader, "DepartmentId") && reader["DepartmentId"] != DBNull.Value ? Guid.Parse(reader["DepartmentId"].ToString()!) : Guid.Empty
                    });
                }
            }
            catch
            {
            }
            return list;
        }

        public List<Role> GetRoles()
        {
            var list = new List<Role>();
            using var conn = _factory.Create();
            conn.Open();
            try
            {
                const string sql = "SELECT * FROM Role";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Role
                    {
                        Id = HasColumn(reader, "Id") ? Guid.Parse(reader["Id"].ToString()!) : (HasColumn(reader, "RoleId") ? Guid.Parse(reader["RoleId"].ToString()!) : Guid.NewGuid()),
                        RoleName = HasColumn(reader, "RoleName") ? reader["RoleName"].ToString()! : (HasColumn(reader, "Name") ? reader["Name"].ToString()! : "")
                    });
                }
            }
            catch
            {
            }
            return list;
        }

        public List<Role> GetRolesByEmployeeId(Guid employeeId)
        {
            var list = new List<Role>();
            using var conn = _factory.Create();
            conn.Open();
            try
            {
                const string sql = @"
                    SELECT r.Id, r.RoleName
                    FROM Role r
                    INNER JOIN UserRole ur ON r.Id = ur.RoleId
                    WHERE ur.EmployeeId = @EmployeeId";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Role
                    {
                        Id = Guid.Parse(reader["Id"].ToString()!),
                        RoleName = reader["RoleName"].ToString()!
                    });
                }
            }
            catch
            {
            }
            return list;
        }

        public void Create(Employee employee, List<Guid> roleIds)
        {
            using var conn = _factory.Create();
            conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                const string sqlEmp = @"
                    INSERT INTO Employee (Id, EmployeeCode, FullName, Email, PasswordHash, IsActive, PositionId, CreatedDate, PermissionVersion)
                    VALUES (@Id, @EmployeeCode, @FullName, @Email, @PasswordHash, @IsActive, @PositionId, @CreatedDate, @PermissionVersion)";

                using (var cmd = new SqlCommand(sqlEmp, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Id", employee.Id == Guid.Empty ? Guid.NewGuid() : employee.Id);
                    cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode ?? "");
                    cmd.Parameters.AddWithValue("@FullName", employee.FullName ?? "");
                    cmd.Parameters.AddWithValue("@Email", employee.Email ?? "");
                    cmd.Parameters.AddWithValue("@PasswordHash", employee.PasswordHash ?? "");
                    cmd.Parameters.AddWithValue("@IsActive", employee.IsActive);
                    cmd.Parameters.AddWithValue("@PositionId", (object?)employee.PositionId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedDate", employee.CreatedDate == default ? DateTime.Now : employee.CreatedDate);
                    cmd.Parameters.AddWithValue("@PermissionVersion", Guid.NewGuid());
                    cmd.ExecuteNonQuery();
                }

                if (roleIds != null && roleIds.Count > 0)
                {
                    foreach (var roleId in roleIds)
                    {
                        const string sqlRole = "INSERT INTO UserRole (EmployeeId, RoleId) VALUES (@EmployeeId, @RoleId)";
                        using var cmdRole = new SqlCommand(sqlRole, conn, tran);
                        cmdRole.Parameters.AddWithValue("@EmployeeId", employee.Id);
                        cmdRole.Parameters.AddWithValue("@RoleId", roleId);
                        cmdRole.ExecuteNonQuery();
                    }
                }
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void Update(Employee employee, List<Guid> roleIds)
        {
            using var conn = _factory.Create();
            conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                const string sqlEmp = @"
                    UPDATE Employee
                    SET EmployeeCode = @EmployeeCode,
                        FullName = @FullName,
                        Email = @Email,
                        IsActive = @IsActive,
                        PositionId = @PositionId,
                        PermissionVersion = @PermissionVersion
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(sqlEmp, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Id", employee.Id);
                    cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode ?? "");
                    cmd.Parameters.AddWithValue("@FullName", employee.FullName ?? "");
                    cmd.Parameters.AddWithValue("@Email", employee.Email ?? "");
                    cmd.Parameters.AddWithValue("@IsActive", employee.IsActive);
                    cmd.Parameters.AddWithValue("@PositionId", (object?)employee.PositionId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PermissionVersion", Guid.NewGuid());
                    cmd.ExecuteNonQuery();
                }

                if (roleIds != null)
                {
                    const string sqlDelRoles = "DELETE FROM UserRole WHERE EmployeeId = @EmployeeId";
                    using (var cmdDel = new SqlCommand(sqlDelRoles, conn, tran))
                    {
                        cmdDel.Parameters.AddWithValue("@EmployeeId", employee.Id);
                        cmdDel.ExecuteNonQuery();
                    }

                    foreach (var roleId in roleIds)
                    {
                        const string sqlInsRole = "INSERT INTO UserRole (EmployeeId, RoleId) VALUES (@EmployeeId, @RoleId)";
                        using var cmdIns = new SqlCommand(sqlInsRole, conn, tran);
                        cmdIns.Parameters.AddWithValue("@EmployeeId", employee.Id);
                        cmdIns.Parameters.AddWithValue("@RoleId", roleId);
                        cmdIns.ExecuteNonQuery();
                    }
                }
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void Delete(Guid id)
        {
            using var conn = _factory.Create();
            conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                const string sqlDelRoles = "DELETE FROM UserRole WHERE EmployeeId = @EmployeeId";
                using (var cmdDel = new SqlCommand(sqlDelRoles, conn, tran))
                {
                    cmdDel.Parameters.AddWithValue("@EmployeeId", id);
                    cmdDel.ExecuteNonQuery();
                }

                const string sqlEmp = "DELETE FROM Employee WHERE Id = @Id";
                using (var cmdEmp = new SqlCommand(sqlEmp, conn, tran))
                {
                    cmdEmp.Parameters.AddWithValue("@Id", id);
                    cmdEmp.ExecuteNonQuery();
                }
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            using var conn = _factory.Create();

            await conn.OpenAsync();

            const string sql = @"
                SELECT COUNT(1)
                FROM Employee
                WHERE Email = @Email";

            using SqlCommand command = new SqlCommand(sql, conn);
            command.Parameters.AddWithValue("@Email", email);

            int count = (int)await command.ExecuteScalarAsync();

            return count > 0;
        }

        public async Task<bool> CheckEmployeeCodeExistsAsync(string employeeCode)
        {
            using var conn = _factory.Create();

            await conn.OpenAsync();

            const string sql = @"
                SELECT COUNT(1)
                FROM Employee
                WHERE EmployeeCode = @EmployeeCode";

            using SqlCommand command = new SqlCommand(sql, conn);
            command.Parameters.AddWithValue("@EmployeeCode", employeeCode);

            int count = (int)await command.ExecuteScalarAsync();

            return count > 0;
        }
    }
}
