using khaosat_fe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace khaosat_fe.Controllers
{
    [Authorize(Roles = "Admin,Quản lý")]
    public class EmployeeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmployeeController(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            var baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:44327/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private void AttachBearerToken()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var token = user?.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetEmployees()
        {
            AttachBearerToken();
            try
            {
                var employees = await _httpClient.GetFromJsonAsync<List<EmployeeViewModel>>("api/employee");
                return Json(employees);
            }
            catch
            {
                return Json(new List<EmployeeViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedEmployees([FromQuery] EmployeeFilterViewModel filter)
        {
            AttachBearerToken();
            try
            {
                var queryParams = new List<string>();
                if (filter.PageNumber > 0) queryParams.Add($"pageNumber={filter.PageNumber}");
                if (filter.PageSize > 0) queryParams.Add($"pageSize={filter.PageSize}");
                if (!string.IsNullOrWhiteSpace(filter.SearchKeyword)) queryParams.Add($"searchKeyword={Uri.EscapeDataString(filter.SearchKeyword)}");
                if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty) queryParams.Add($"departmentId={filter.DepartmentId.Value}");
                if (filter.PositionId.HasValue && filter.PositionId.Value != Guid.Empty) queryParams.Add($"positionId={filter.PositionId.Value}");
                if (filter.IsActive.HasValue) queryParams.Add($"isActive={filter.IsActive.Value}");
                if (!string.IsNullOrWhiteSpace(filter.SortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(filter.SortBy)}");
                queryParams.Add($"isDescending={filter.IsDescending}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var pagedResult = await _httpClient.GetFromJsonAsync<PagedResultViewModel<EmployeeViewModel>>($"api/employee/paged{queryString}");

                var result = pagedResult ?? new PagedResultViewModel<EmployeeViewModel>();
                return Json(new
                {
                    data = result.Data,
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                });
            }
            catch
            {
                return Json(new PagedResultViewModel<EmployeeViewModel>());
            }
        }

        public async Task<IActionResult> GetRoles()
        {
            AttachBearerToken();
            try
            {
                var roles = await _httpClient.GetFromJsonAsync<List<RoleViewModel>>("api/employee/roles");
                    return Json(roles);
                }
            catch
            {
                return Json(new List<RoleViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string values)
        {
            AttachBearerToken();
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var model = System.Text.Json.JsonSerializer.Deserialize<EmployeeViewModel>(values, options) ?? new EmployeeViewModel();
                var response = await _httpClient.PostAsJsonAsync("api/employee", model);
                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }
                var error = await response.Content.ReadAsStringAsync();

                return BadRequest(error);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] Guid key, [FromForm] string values)
        {
            AttachBearerToken();
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var model = System.Text.Json.JsonSerializer.Deserialize<EmployeeViewModel>(values, options) ?? new EmployeeViewModel();
                model.Id = key;
                var response = await _httpClient.PutAsJsonAsync($"api/employee/{key}", model);
                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }
                return BadRequest("Cập nhật nhân sự thất bại");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromForm] Guid key)
        {
            AttachBearerToken();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/employee/{key}");
                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }
                return BadRequest("Xóa nhân sự thất bại");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmailExists(string email, string? employeeCode)
        {
            AttachBearerToken();
            

            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(true);
            }

            try
            {
                var encodedEmail = Uri.EscapeDataString(email);

                var exists = await _httpClient.GetFromJsonAsync<bool>(
                     $"api/employee/check-email-exists?email={encodedEmail}&employeeCode={employeeCode}"
                );

                if (exists)
                {
                    return Json("Email đã được đăng ký");
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                return Json($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmployeeCodeExists(string employeeCode)
        {
            AttachBearerToken();

            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                return Json(true);
            }

            try
            {
                var encodedCode = Uri.EscapeDataString(employeeCode);

                var exists = await _httpClient.GetFromJsonAsync<bool>(
                     $"api/employee/check-employeeCode-exists?employeeCode={encodedCode}"
                );

                if (exists)
                {
                    return Json("Mã nhân viên đã tồn tại");
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                return Json($"Có lỗi xảy ra: {ex.Message}");
            }
        }
    }
}
