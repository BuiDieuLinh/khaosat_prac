using Microsoft.AspNetCore.Mvc;
using khaosat_api.DTOs;
using khaosat_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace khaosat_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/survey")]
    public class SurveyController : Controller
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpGet]
        public IActionResult GetSurveys()
        {
            Guid? currentUserId = null;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid parsedId))
            {
                currentUserId = parsedId;
            }
            var surveys = _surveyService.GetSurveys(currentUserId);
            return Ok(surveys);
        }

        [HttpGet("{id}")]
        public IActionResult GetSurveyDetail(Guid id)
        {
            Guid? currentUserId = null;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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

        [HttpPost("submit")]
        public IActionResult SubmitSurvey(SurveySubmitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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
                _surveyService.SubmitSurvey(dto);
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
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, SurveyCreateNestedDto dto)
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
    }
}
