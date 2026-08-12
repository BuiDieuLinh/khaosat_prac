using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using khaosat_fe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace khaosat_fe.Controllers
{
    [Authorize]
    public class SurveyController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SurveyController(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        [HttpGet]
        public async Task<IActionResult> GetSurveys()
        {
            AttachBearerToken();
            try
            {
                var surveys = await _httpClient.GetFromJsonAsync<List<SurveyViewModel>>("api/survey");
                return Json(surveys);
            }
            catch
            {
                return Json(new List<SurveyViewModel>());
            }
        }


        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedSurveys([FromQuery] SurveyFilterDto filter)
        {
            AttachBearerToken();

            try
            {
                var queryParams = new List<string>();

                if (filter.PageNumber > 0)
                    queryParams.Add($"pageNumber={filter.PageNumber}");
                if (filter.PageSize > 0)
                    queryParams.Add($"pageSize={filter.PageSize}");
                if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                    queryParams.Add($"searchKeyword={Uri.EscapeDataString(filter.SearchKeyword)}");
                if (filter.Status.HasValue)
                    queryParams.Add($"status={(int)filter.Status.Value}");
                if (!string.IsNullOrWhiteSpace(filter.SortBy))
                    queryParams.Add($"sortBy={Uri.EscapeDataString(filter.SortBy)}");
                queryParams.Add($"isDescending={filter.IsDescending}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                var pagedResult = await _httpClient.GetFromJsonAsync<PagedResultViewModel<SurveyViewModel>>(
                    $"api/survey/paged{queryString}"
                );

                var result = pagedResult ?? new PagedResultViewModel<SurveyViewModel>();

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
                return Json(new PagedResultViewModel<SurveyViewModel>());
            }
        }


        public async Task<IActionResult> Detail(Guid id)
        {
            AttachBearerToken();
            try
            {
                var response = await _httpClient.GetAsync($"api/survey/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var detail = await response.Content.ReadFromJsonAsync<SurveyDetailViewModel>();
                    if (detail != null) return View(detail);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "Bạn không thuộc đối tượng tham gia cuộc khảo sát này!";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Không tìm thấy cuộc khảo sát hoặc bạn không có quyền truy cập.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin khảo sát.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SurveySubmitViewModel model)
        {
            if (model == null || model.SurveyId == Guid.Empty)
            {
                return BadRequest("Invalid data");
            }

            AttachBearerToken();
            try
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdStr, out Guid employeeId))
                {
                    model.EmployeeId = employeeId;
                }
                else
                {
                    return Unauthorized("Không tìm thấy thông tin nhân viên hợp lệ.");
                }

                var response = await _httpClient.PostAsJsonAsync("api/survey/submit", model);
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { message = "Submitted successfully" });
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var err = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    var msg = err != null && err.ContainsKey("message") ? err["message"] : "Bạn không thuộc đối tượng tham gia khảo sát này.";
                    return StatusCode(403, new { message = msg });
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var err = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    var msg = err != null && err.ContainsKey("message") ? err["message"] : "Bạn đã hết số lần làm khảo sát";
                    return StatusCode(409, new { message = msg });
                }
                return BadRequest("Failed to submit survey");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Quản lý")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> CreateNested([FromBody] SurveyCreateNestedViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AttachBearerToken();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/survey/create-nested", model);
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { message = "Survey created successfully" });
                }
                var errorMsg = await response.Content.ReadAsStringAsync();
                return BadRequest(errorMsg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> Edit(Guid id)
        {
            AttachBearerToken();
            try
            {
                var detail = await _httpClient.GetFromJsonAsync<SurveyDetailViewModel>($"api/survey/{id}");
                if (detail == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khảo sát.";
                    return RedirectToAction("Index");
                }

                if (detail.Status == 2)
                {
                    TempData["ErrorMessage"] = "Khảo sát đã được đóng.";
                    return RedirectToAction("Index");
                }

                if (detail.Status == 1 && detail.StartDate.HasValue && detail.StartDate.Value <= DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Khảo sát đã bắt đầu nên không thể chỉnh sửa.";
                    return RedirectToAction("Index");
                }

                return View(detail);
            }
            catch
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin khảo sát.";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> UpdateNested(Guid id, [FromBody] SurveyCreateNestedViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AttachBearerToken();
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/survey/{id}", model);
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { message = "Survey updated successfully" });
                }
                var errorMsg = await response.Content.ReadAsStringAsync();
                return BadRequest(errorMsg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Survey/Clone/{id}")]
        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> Clone(Guid id)
        {
            AttachBearerToken();
            try
            {
                var response = await _httpClient.PostAsync($"api/survey/{id}/clone", null);
                if (response.IsSuccessStatusCode)
                {
                    var cloned = await response.Content.ReadFromJsonAsync<SurveyViewModel>();
                    return Ok(cloned);
                }
                var errorMsg = await response.Content.ReadAsStringAsync();
                return BadRequest(errorMsg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("Survey/Close/{id}")]
        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> Close(Guid id)
        {
            AttachBearerToken();
            try
            {
                var response = await _httpClient.PutAsync($"api/survey/{id}/close", null);
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { message = "Đã đóng khảo sát thành công." });
                }
                var errorMsg = await response.Content.ReadAsStringAsync();
                return BadRequest(errorMsg);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
