using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using khaosat_fe.Models;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;

namespace khaosat_fe.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:44327/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Survey");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ tài khoản và mật khẩu." });
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);
                if (response.IsSuccessStatusCode)
                {
                    var authData = await response.Content.ReadFromJsonAsync<AuthResponseViewModel>();
                    if (authData != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, authData.Id.ToString()),
                            new Claim(ClaimTypes.Name, authData.FullName),
                            new Claim(ClaimTypes.Email, authData.Email),
                            new Claim(ClaimTypes.UserData, authData.EmployeeCode),
                            new Claim("Token", authData.Token) // Store JWT token in cookie
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(180)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return Json(new { success = true });
                    }
                }
                
                var errorMsg = await response.Content.ReadAsStringAsync();
                string cleanMsg = "Tài khoản hoặc mật khẩu không chính xác.";
                try 
                {
                    var errObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonNode>(errorMsg);
                    if (errObj?["message"] != null)
                    {
                        cleanMsg = errObj["message"]!.ToString();
                    }
                }
                catch {}
                
                return Json(new { success = false, message = cleanMsg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
