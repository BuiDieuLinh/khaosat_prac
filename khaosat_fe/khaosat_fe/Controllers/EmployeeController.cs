using khaosat_fe.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace khaosat_fe.Controllers
{
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
                return BadRequest("Thêm nhân sự thất bại");
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
    }
}
