using khaosat_fe.Models;
using Microsoft.AspNetCore.Mvc;

namespace khaosat_fe.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DepartmentController(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<IActionResult> GetDepartments()
        {
            AttachBearerToken();
            try
            {
                var departments = await _httpClient.GetFromJsonAsync<List<DepartmentViewModel>>("api/department");
                return Json(departments);
            }
            catch
            {
                return Json(new List<DepartmentViewModel>());
            }
        }

        public async Task<IActionResult> GetPositions(Guid? departmentId)
        {
            AttachBearerToken();
            try
            {
                var url = departmentId.HasValue && departmentId.Value != Guid.Empty
                    ? $"api/department/positions?departmentId={departmentId.Value}"
                    : "api/department/positions";
                var positions = await _httpClient.GetFromJsonAsync<List<PositionViewModel>>(url);
                return Json(positions);
            }
            catch
            {
                return Json(new List<PositionViewModel>());
            }
        }
    }
}
