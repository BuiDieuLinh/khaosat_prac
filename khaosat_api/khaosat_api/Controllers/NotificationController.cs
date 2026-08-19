using khaosat_api.DTOs;
using khaosat_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace khaosat_api.Controllers
{
    [ApiController]
    [Route("api/notification")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notification;

        public NotificationController(INotificationService service)
        {
            _notification = service;
        }

        [HttpGet]
        public IActionResult GetNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? typeFilter = null)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var result = _notification.GetNotificationsByUserId(
                userId,
                pageNumber,
                pageSize,
                typeFilter);

            return Ok(result);
        }

        [HttpPost]
        public IActionResult Add(
            [FromBody] NotificationDto noti)
        {
            if (noti == null)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu thông báo không hợp lệ."
                });
            }

            try
            {
                noti.CreatedDate = DateTime.UtcNow;
                _notification.Add(noti);

                return Ok(new
                {
                    message = "Thêm thông báo thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(Guid id)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                _notification.UpdateStatus(id, userId);

                return Ok(new
                {
                    message = "Đã đánh dấu thông báo là đã đọc."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
