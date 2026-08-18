using System.Security.Claims;
using khaosat_api.DTOs;
using khaosat_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace khaosat_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/survey")]
    public class SurveyController : BaseController
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        private string? GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet]
        public IActionResult GetSurveys()
        {
            var userId = CurrentUserId;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid parsedId))
            {
                userId = parsedId;
            }
            var surveys = _surveyService.GetSurveys(userId);
            return Ok(surveys);
        }

        [HttpGet("paged")]
        public IActionResult GetPagedSurveys([FromQuery] SurveyFilterDto filter)
        {
            var userId = CurrentUserId;
            bool isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Quản lý");

            var result = _surveyService.GetSurveys(filter, userId, isAdminOrManager);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetSurveyDetail(Guid id)
        {
            Guid? currentUserId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid parsedId))
            {
                currentUserId = parsedId;
            }

            try
            {
                var survey = _surveyService.GetSurveyDetail(id, currentUserId);
                if (survey == null)
                {
                    return NotFound(new { message = "Survey not found" });
                }
                return Ok(survey);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "NOT_IN_TARGET_AUDIENCE")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Bạn không thuộc đối tượng tham gia khảo sát này." });
                }
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("public/{token}")]
        public IActionResult GetPublicSurveyDetail(string token)
        {
            try
            {
                var survey = _surveyService.GetPublicSurveyDetail(token);
                if (survey == null)
                {
                    return NotFound(new { message = "Khảo sát không tồn tại hoặc không khả dụng." });
                }
                return Ok(survey);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("submit")]
        public IActionResult SubmitSurvey(SurveySubmitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid employeeId))
            {
                dto.EmployeeId = employeeId;
            }
            else
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin nhân viên hợp lệ trong Token." });
            }

            try
            {
                _surveyService.SubmitSurvey(dto, GetCurrentUsername(), GetClientIpAddress(), GetUserAgent());
                return Ok(new { message = "Survey submitted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "NOT_IN_TARGET_AUDIENCE")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Bạn không thuộc đối tượng tham gia khảo sát này." });
                }
                if (ex.Message == "MAX_ATTEMPTS_EXCEEDED")
                {
                    return StatusCode(StatusCodes.Status409Conflict, new { message = "Bạn đã hết số lần làm khảo sát" });
                }
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("public/submit")]
        public IActionResult SubmitPublicSurvey(SurveySubmitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _surveyService.SubmitPublicSurvey(dto, GetClientIpAddress(), GetUserAgent());
                return Ok(new { message = "Public survey submitted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Quản lý")]
        [HttpPost("create-nested")]
        public IActionResult CreateNested(SurveyCreateNestedDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _surveyService.CreateNested(dto);
            return Ok(new { message = "Nested survey created successfully" });
        }

        [Authorize(Roles = "Admin,Quản lý")]
        [HttpPatch("{id}")]
        public IActionResult Update(Guid id, [FromBody] SurveyUpdateNestedDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _surveyService.UpdateNested(id, dto);
                return Ok(new { message = "Survey updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Quản lý")]
        [HttpPut("{id}/access-type")]
        public IActionResult ChangeAccessType(Guid id, [FromBody] ChangeAccessTypeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _surveyService.ChangeAccessType(id, dto.AccessType);
                return Ok(new { message = "Access type updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Quản lý")]
        [HttpPut("{id}/anonymous-mode")]
        public IActionResult ChangeAnonymousMode(Guid id, [FromBody] ChangeAnonymousModeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _surveyService.ChangeAnonymousMode(id, dto.AnonymousMode);
                return Ok(new { message = "Anonymous mode updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Quản lý")]
        [HttpPost("{id}/clone")]
        public IActionResult Clone(Guid id)
        {
            try
            {
                var result = _surveyService.CloneSurvey(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Quản lý")]
        [HttpPut("{id}/close")]
        public IActionResult Close(Guid id)
        {
            try
            {
                _surveyService.CloseSurvey(id);
                return Ok(new { message = "Đã đóng khảo sát thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Quản lý")]
        [HttpGet("{id}/report")]
        public IActionResult GetSurveyReport(Guid id)
        {
            try
            {
                var report = _surveyService.GetSurveyReport(id);
                return Ok(report);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("audit-logs")]
        public IActionResult GetAuditLogs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? action = null, [FromQuery] string? search = null)
        {
            var logs = _surveyService.GetAuditLogs(pageNumber, pageSize, action, search);
            return Ok(logs);
        }
    }
}
