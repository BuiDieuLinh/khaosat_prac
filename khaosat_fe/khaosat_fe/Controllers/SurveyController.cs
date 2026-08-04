using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using khaosat_fe.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

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

        public async Task<IActionResult> Detail(Guid id)
        {
            AttachBearerToken();
            try
            {
                var detail = await _httpClient.GetFromJsonAsync<SurveyDetailViewModel>($"api/survey/{id}");
                return View(detail);
            }
            catch
            {
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
                // Ghi đè EmployeeId từ Token Claims của User đang login
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
                return BadRequest("Failed to submit survey");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
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

                if (detail.StartDate.HasValue && detail.StartDate.Value <= DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Không thể chỉnh sửa khảo sát đã công khai (sau ngày bắt đầu).";
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
    }
}
