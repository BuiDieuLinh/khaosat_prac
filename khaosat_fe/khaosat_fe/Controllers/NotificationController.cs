using khaosat_fe.Models;
using Microsoft.AspNetCore.Mvc;

namespace khaosat_fe.Controllers
{
    public class NotificationController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationController(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            var baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:44327/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private void AttachBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificationsByUserId([FromQuery] int pageSize = 10)
        {
            AttachBearerToken();

            try
            {
                pageSize = Math.Clamp(pageSize, 1, 100);
                var result = await _httpClient.GetFromJsonAsync<PagedResultViewModel<NotificationViewModel>>(
                    $"api/notification?pageNumber=1&pageSize={pageSize}");

                result ??= new PagedResultViewModel<NotificationViewModel>();
                return Json(new { data = result.Data, totalCount = result.TotalCount });
            }
            catch
            {
                return Json(new { data = new List<NotificationViewModel>(), totalCount = 0 });
            }
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateStatus(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Id thông báo không hợp lệ.");
            }

            AttachBearerToken();
            try
            {
                var response = await _httpClient.PatchAsync($"api/notification/{id}/status", null);
                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }

                return BadRequest("Cập nhật trạng thái thông báo thất bại.");
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
