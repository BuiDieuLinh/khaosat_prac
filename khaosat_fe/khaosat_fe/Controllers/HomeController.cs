using khaosat_fe.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace khaosat_fe.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
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

        public async Task<IActionResult> Index()
        {
            var auditLogs = new PagedResultViewModel<AuditLogViewModel>();
            if (User.IsInRole("Admin"))
            {
                AttachBearerToken();
                try
                {
                    var response = await _httpClient.GetFromJsonAsync<PagedResultViewModel<AuditLogViewModel>>("api/survey/audit-logs?pageNumber=1&pageSize=10");
                    if (response != null)
                    {
                        auditLogs = response;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching audit logs for home page.");
                }
            }

            return View(auditLogs);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
