using khaosat_api.DTOs;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using khaosat_api.Services.Interfaces;

namespace khaosat_api.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly ISurveyRepository _repository;
        private readonly ISurveyElementRepository _elementRepository;
        private readonly ISurveyElementOptionRepository _optionRepository;
        private readonly ISurveyResponseRepository _responseRepository;
        private readonly ISurveyAnswerRepository _answerRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public SurveyService(
            ISurveyRepository repository,
            ISurveyElementRepository elementRepository,
            ISurveyElementOptionRepository optionRepository,
            ISurveyResponseRepository responseRepository,
            ISurveyAnswerRepository answerRepository,
            IEmployeeRepository employeeRepository)
        {
            _repository = repository;
            _elementRepository = elementRepository;
            _optionRepository = optionRepository;
            _responseRepository = responseRepository;
            _answerRepository = answerRepository;
            _employeeRepository = employeeRepository;
        }

        public List<SurveyDto> GetSurveys()
        {
            var surveys = _repository.GetAll();
            var activeEmployeeCount = _employeeRepository.GetActiveEmployeeCount();
            var completedCounts = _responseRepository.GetCompletedCounts();

            return surveys.Select(x => {
                var status = x.Status;
                if (x.Status == 1 && x.EndDate.HasValue && x.EndDate.Value < DateTime.Now)
                {
                    status = 0; 
                    _repository.UpdateStatus(x.Id, 0); 
                }

                completedCounts.TryGetValue(x.Id, out int completedCount);
                int incompleteCount = Math.Max(0, activeEmployeeCount - completedCount);
                double completionRate = activeEmployeeCount > 0 ? Math.Round((double)completedCount / activeEmployeeCount * 100, 2) : 0;

                return new SurveyDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = status,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate,
                    TotalResponses = activeEmployeeCount,
                    CompletedCount = completedCount,
                    IncompleteCount = incompleteCount,
                    CompletionRate = completionRate
                };
            }).ToList();
        }

        public SurveyDetailDto? GetSurveyDetail(Guid id)
        {
            var survey = _repository.GetById(id);
            if (survey == null)
            {
                return null;
            }

            var status = survey.Status;
            if (survey.Status == 1 && survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now)
            {
                status = 0; 
                _repository.UpdateStatus(survey.Id, 0); 
            }

            var elements = _elementRepository.GetBySurveyId(id);
            var elementDetails = new List<SurveyElementDetailDto>();

            foreach (var element in elements)
            {
                var options = _optionRepository.GetByElementId(element.Id);
                elementDetails.Add(new SurveyElementDetailDto
                {
                    Id = element.Id,
                    SurveyId = element.SurveyId,
                    FieldName = element.FieldName,
                    SortOrder = element.SortOrder,
                    ConfigType = element.ConfigType,
                    Options = options.Select(o => new SurveyElementOptionDto
                    {
                        Id = o.Id,
                        ElementId = o.ElementId,
                        Value = o.Value,
                        DisplayText = o.DisplayText,
                        SortOrder = o.SortOrder,
                        IsDefault = o.IsDefault,
                        IsActive = o.IsActive
                    }).ToList()
                });
            }

            return new SurveyDetailDto
            {
                Id = survey.Id,
                Code = survey.Code,
                Name = survey.Name,
                Description = survey.Description,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                Status = status,
                CreatedDate = survey.CreatedDate,
                UpdatedDate = survey.UpdatedDate,
                Elements = elementDetails
            };
        }

        public void SubmitSurvey(SurveySubmitDto dto)
        {
            var survey = _repository.GetById(dto.SurveyId);
            if (survey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            if (survey.Status == 0 || (survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now))
            {
                throw new InvalidOperationException("Cuộc khảo sát đã kết thúc hoặc tạm đóng.");
            }

            var responseId = Guid.NewGuid();
            var response = new SurveyResponse
            {
                Id = responseId,
                SurveyId = dto.SurveyId,
                EmployeeId = dto.EmployeeId,
                SubmitDate = DateTime.Now,
                Status = 1
            };

            _responseRepository.Add(response);

            if (dto.Answers != null)
            {
                foreach (var ansDto in dto.Answers)
                {
                    var answer = new SurveyAnswer
                    {
                        Id = Guid.NewGuid(),
                        ResponseId = responseId,
                        ElementId = ansDto.ElementId,
                        OptionId = ansDto.OptionId,
                        Value = ansDto.Value
                    };

                    _answerRepository.Add(answer);
                }
            }
        }

        public void CreateNested(SurveyCreateNestedDto dto)
        {
            var surveyId = Guid.NewGuid();
            var survey = new Survey
            {
                Id = surveyId,
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                CreatedDate = DateTime.Now,
                UpdatedDate = null
            };

            _repository.Add(survey);

            if (dto.Elements != null)
            {
                foreach (var elDto in dto.Elements)
                {
                    var elementId = Guid.NewGuid();
                    var element = new SurveyElement
                    {
                        Id = elementId,
                        SurveyId = surveyId,
                        FieldName = elDto.FieldName,
                        SortOrder = elDto.SortOrder,
                        ConfigType = elDto.ConfigType
                    };

                    _elementRepository.Add(element);

                    if (elDto.Options != null)
                    {
                        foreach (var optDto in elDto.Options)
                        {
                            var option = new SurveyElementOption
                            {
                                Id = Guid.NewGuid(),
                                ElementId = elementId,
                                Value = optDto.Value,
                                DisplayText = optDto.DisplayText,
                                SortOrder = optDto.SortOrder,
                                IsDefault = optDto.IsDefault,
                                IsActive = optDto.IsActive
                            };

                            _optionRepository.Add(option);
                        }
                    }
                }
            }
        }

        public void UpdateNested(Guid id, SurveyCreateNestedDto dto)
        {
            var survey = _repository.GetById(id);
            if (survey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            if (survey.StartDate.HasValue && survey.StartDate.Value <= DateTime.Now)
            {
                throw new InvalidOperationException("Không thể chỉnh sửa khảo sát đã công khai (sau ngày bắt đầu).");
            }

            survey.Code = dto.Code;
            survey.Name = dto.Name;
            survey.Description = dto.Description;
            survey.StartDate = dto.StartDate;
            survey.EndDate = dto.EndDate;
            survey.Status = dto.Status;
            survey.UpdatedDate = DateTime.Now;

            _repository.Update(survey);

            _repository.DeleteElementsAndOptions(id);

            if (dto.Elements != null)
            {
                foreach (var elDto in dto.Elements)
                {
                    var elementId = Guid.NewGuid();
                    var element = new SurveyElement
                    {
                        Id = elementId,
                        SurveyId = id,
                        FieldName = elDto.FieldName,
                        SortOrder = elDto.SortOrder,
                        ConfigType = elDto.ConfigType
                    };

                    _elementRepository.Add(element);

                    if (elDto.Options != null)
                    {
                        foreach (var optDto in elDto.Options)
                        {
                            var option = new SurveyElementOption
                            {
                                Id = Guid.NewGuid(),
                                ElementId = elementId,
                                Value = optDto.Value,
                                DisplayText = optDto.DisplayText,
                                SortOrder = optDto.SortOrder,
                                IsDefault = optDto.IsDefault,
                                IsActive = optDto.IsActive
                            };

                            _optionRepository.Add(option);
                        }
                    }
                }
            }
        }
    }
}
