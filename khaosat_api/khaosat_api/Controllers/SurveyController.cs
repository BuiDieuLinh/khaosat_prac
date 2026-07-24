using Microsoft.AspNetCore.Mvc;
using khaosat_api.DTOs;
using khaosat_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace khaosat_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/survey")]
    [Route("survey")]
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
            var surveys = _surveyService.GetSurveys();
            return Ok(surveys);
        }

        [HttpGet("{id}")]
        public IActionResult GetSurveyDetail(Guid id)
        {
            var survey = _surveyService.GetSurveyDetail(id);
            if (survey == null)
            {
                return NotFound(new { message = "Survey not found" });
            }
            return Ok(survey);
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
                return BadRequest(new { message = ex.Message });
            }
        }

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
    }
}
