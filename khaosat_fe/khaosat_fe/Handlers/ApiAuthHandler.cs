using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace khaosat_fe.Handlers
{
    public class ApiAuthHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var user = httpContext.User;
                var token = user?.FindFirst("Token")?.Value;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                if (httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent) && !string.IsNullOrWhiteSpace(userAgent))
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", (string?)userAgent);
                }

                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrEmpty(clientIp))
                {
                    request.Headers.TryAddWithoutValidation("X-Forwarded-For", clientIp);
                }
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized && httpContext != null)
            {
                string reason = "TokenExpired";
                if (response.Headers.TryGetValues("X-Auth-Reason", out var values))
                {
                    reason = string.Join(",", values);
                }
                httpContext.Response.Headers["X-Auth-Reason"] = reason;
                
                // Sign out FE Cookie session immediately to avoid infinite redirect loops
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return response;
        }
    }
}
