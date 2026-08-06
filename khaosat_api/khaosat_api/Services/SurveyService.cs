using khaosat_api.Data;
using khaosat_api.DTOs;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using khaosat_api.Services.Interfaces;
using System.Data.SqlClient;
using System.Transactions;

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
        private readonly ISurveyTargetRepository _targetRepository;

        public SurveyService(
            ISurveyRepository repository,
            ISurveyElementRepository elementRepository,
            ISurveyElementOptionRepository optionRepository,
            ISurveyResponseRepository responseRepository,
            ISurveyAnswerRepository answerRepository,
            IEmployeeRepository employeeRepository,
            ISurveyTargetRepository targetRepository)
        {
            _repository = repository;
            _elementRepository = elementRepository;
            _optionRepository = optionRepository;
            _responseRepository = responseRepository;
            _answerRepository = answerRepository;
            _employeeRepository = employeeRepository;
            _targetRepository = targetRepository;
        }

        private bool IsEmployeeInTargetAudience(Guid surveyId, EmployeeResponse employee)
        {
            if (employee.Roles != null && employee.Roles.Any(r => r.RoleName == "Admin" || r.RoleName == "Quản lý"))
            {
                return true;
            }

            var targets = _targetRepository.GetBySurveyId(surveyId);
            if (targets == null || targets.Count == 0)
            {
                return true;
            }

            foreach (var t in targets)
            {
                if (t.TargetType == 1) return true;
                if (t.TargetType == 2 && employee.DepartmentId.HasValue && t.TargetId == employee.DepartmentId.Value) return true;
                if (t.TargetType == 3 && employee.PositionId.HasValue && t.TargetId == employee.PositionId.Value) return true;
                if (t.TargetType == 4 && t.TargetId == employee.Id) return true;
            }

            return false;
        }

        public List<SurveyDto> GetSurveys(Guid? currentUserId = null)
        {
            var surveys = _repository.GetAll();
            var activeEmployeeCount = _employeeRepository.GetActiveEmployeeCount();
            var completedCounts = _responseRepository.GetCompletedCounts();

            EmployeeResponse? employee = null;
            if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
            {
                employee = _employeeRepository.GetByIdAsync(currentUserId.Value).GetAwaiter().GetResult();
            }

            var resultList = new List<SurveyDto>();

            foreach (var x in surveys)
            {
                if (employee != null && !IsEmployeeInTargetAudience(x.Id, employee))
                {
                    continue; // Skip survey if user is not in target audience
                }

                var targets = _targetRepository.GetBySurveyId(x.Id);
                var status = x.Status;
                if (x.Status == 1 && x.EndDate.HasValue && x.EndDate.Value < DateTime.Now)
                {
                    status = 0; 
                    _repository.UpdateStatus(x.Id, 0); 
                }

                completedCounts.TryGetValue(x.Id, out int completedCount);
                int incompleteCount = Math.Max(0, activeEmployeeCount - completedCount);
                double completionRate = activeEmployeeCount > 0 ? Math.Round((double)completedCount / activeEmployeeCount * 100, 2) : 0;

                resultList.Add(new SurveyDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = status,
                    MaxAttempts = x.MaxAttempts,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate,
                    Targets = targets.Select(t => new SurveyTargetDto { TargetType = t.TargetType, TargetId = t.TargetId }).ToList(),
                    TotalResponses = activeEmployeeCount,
                    CompletedCount = completedCount,
                    IncompleteCount = incompleteCount,
                    CompletionRate = completionRate
                });
            }

            return resultList;
        }

        public SurveyDetailDto? GetSurveyDetail(Guid id, Guid? currentUserId = null)
        {
            var survey = _repository.GetById(id);
            if (survey == null)
            {
                return null;
            }

            if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
            {
                var employee = _employeeRepository.GetByIdAsync(currentUserId.Value).GetAwaiter().GetResult();
                if (employee != null && !IsEmployeeInTargetAudience(id, employee))
                {
                    throw new InvalidOperationException("NOT_IN_TARGET_AUDIENCE");
                }
            }

            var status = survey.Status;
            if (survey.Status == 1 && survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now)
            {
                status = 0; 
                _repository.UpdateStatus(survey.Id, 0); 
            }

            var targets = _targetRepository.GetBySurveyId(id);
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
                MaxAttempts = survey.MaxAttempts,
                CreatedDate = survey.CreatedDate,
                UpdatedDate = survey.UpdatedDate,
                Targets = targets.Select(t => new SurveyTargetDto { TargetType = t.TargetType, TargetId = t.TargetId }).ToList(),
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

            if (survey.Status != 1 || (survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now))
            {
                throw new InvalidOperationException("Cuộc khảo sát đã kết thúc hoặc chưa phát hành.");
            }

            // Target Audience Validation for Submit
            if (dto.EmployeeId != Guid.Empty)
            {
                var employee = _employeeRepository.GetByIdAsync(dto.EmployeeId).GetAwaiter().GetResult();
                if (employee != null && !IsEmployeeInTargetAudience(dto.SurveyId, employee))
                {
                    throw new InvalidOperationException("NOT_IN_TARGET_AUDIENCE");
                }
            }

            // Max Attempts Validation
            if (survey.MaxAttempts.HasValue && survey.MaxAttempts.Value > 0)
            {
                var userAttemptCount = _responseRepository.GetCountBySurveyAndEmployee(dto.SurveyId, dto.EmployeeId);
                if (userAttemptCount >= survey.MaxAttempts.Value)
                {
                    throw new InvalidOperationException("MAX_ATTEMPTS_EXCEEDED");
                }
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
            using var scope = new TransactionScope();

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
                MaxAttempts = dto.MaxAttempts,
                CreatedDate = DateTime.Now,
                UpdatedDate = null
            };

            _repository.Add(survey);

            if (dto.Targets != null && dto.Targets.Count > 0)
            {
                foreach (var tDto in dto.Targets)
                {
                    var target = new SurveyTarget
                    {
                        Id = Guid.NewGuid(),
                        SurveyId = surveyId,
                        TargetType = tDto.TargetType,
                        TargetId = tDto.TargetType == 1 ? null : tDto.TargetId
                    };
                    _targetRepository.Add(target);
                }
            }

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

            scope.Complete();
        }

        public void UpdateNested(Guid id, SurveyCreateNestedDto dto)
        {
            var existingSurvey = _repository.GetById(id);
            if (existingSurvey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            if (existingSurvey.Status == 2)
            {
                throw new InvalidOperationException("Khảo sát đã được đóng.");
            }

            var responseCount = _responseRepository.GetCountBySurveyId(id);
            if (responseCount > 0)
            {
                throw new InvalidOperationException("Khảo sát đã có người tham gia.");
            }

            if (existingSurvey.Status == 1 && existingSurvey.StartDate.HasValue && existingSurvey.StartDate.Value <= DateTime.Now)
            {
                throw new InvalidOperationException("Khảo sát đã bắt đầu.");
            }

            using var scope = new TransactionScope();

            existingSurvey.Code = dto.Code;
            existingSurvey.Name = dto.Name;
            existingSurvey.Description = dto.Description;
            existingSurvey.StartDate = dto.StartDate;
            existingSurvey.EndDate = dto.EndDate;
            existingSurvey.Status = dto.Status;
            existingSurvey.MaxAttempts = dto.MaxAttempts;
            existingSurvey.UpdatedDate = DateTime.Now;

            _repository.Update(existingSurvey);

            _targetRepository.DeleteBySurveyId(id);
            if (dto.Targets != null && dto.Targets.Count > 0)
            {
                foreach (var tDto in dto.Targets)
                {
                    var target = new SurveyTarget
                    {
                        Id = Guid.NewGuid(),
                        SurveyId = id,
                        TargetType = tDto.TargetType,
                        TargetId = tDto.TargetType == 1 ? null : tDto.TargetId
                    };
                    _targetRepository.Add(target);
                }
            }

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

            scope.Complete();
        }

        private string GenerateUniqueCode()
        {
            var existingCodes = _repository.GetAll().Select(s => s.Code.ToUpper()).ToHashSet();
            for (int i = 1; i <= 999; i++)
            {
                string candidate = $"C{i:D4}";
                if (!existingCodes.Contains(candidate))
                {
                    return candidate;
                }
            }
            return $"C{Random.Shared.Next(1000, 9999)}";
        }

        private string GenerateCloneName(string originalName)
        {
            var existingNames = _repository.GetAll().Select(s => s.Name.Trim()).ToHashSet();
            string candidate = $"{originalName} (Copy)";
            if (!existingNames.Contains(candidate))
            {
                return candidate;
            }

            int counter = 2;
            while (true)
            {
                candidate = $"{originalName} (Copy {counter})";
                if (!existingNames.Contains(candidate))
                {
                    return candidate;
                }
                counter++;
            }
        }

        public SurveyDto CloneSurvey(Guid id)
        {
            var existingSurvey = _repository.GetById(id);
            if (existingSurvey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            var targets = _targetRepository.GetBySurveyId(id) ?? new List<SurveyTarget>();
            var elements = _elementRepository.GetBySurveyId(id) ?? new List<SurveyElement>();
            var optionsMap = new Dictionary<Guid, List<SurveyElementOption>>();
            foreach (var el in elements)
            {
                optionsMap[el.Id] = _optionRepository.GetByElementId(el.Id) ?? new List<SurveyElementOption>();
            }

            var newSurveyId = Guid.NewGuid();
            var newName = GenerateCloneName(existingSurvey.Name);
            var newCode = GenerateUniqueCode();
            var createdDate = DateTime.Now;

            using var scope = new TransactionScope();

            var newSurvey = new Survey
            {
                Id = newSurveyId,
                Code = newCode,
                Name = newName,
                Description = existingSurvey.Description,
                StartDate = existingSurvey.StartDate,
                EndDate = existingSurvey.EndDate,
                Status = 0, // Draft
                MaxAttempts = existingSurvey.MaxAttempts,
                CreatedDate = createdDate,
                UpdatedDate = null
            };

            _repository.Add(newSurvey);

            if (targets.Count > 0)
            {
                foreach (var t in targets)
                {
                    var target = new SurveyTarget
                    {
                        Id = Guid.NewGuid(),
                        SurveyId = newSurveyId,
                        TargetType = t.TargetType,
                        TargetId = t.TargetId
                    };
                    _targetRepository.Add(target);
                }
            }

            foreach (var el in elements)
            {
                var newElementId = Guid.NewGuid();
                var element = new SurveyElement
                {
                    Id = newElementId,
                    SurveyId = newSurveyId,
                    FieldName = el.FieldName,
                    SortOrder = el.SortOrder,
                    ConfigType = el.ConfigType
                };

                _elementRepository.Add(element);

                if (optionsMap.TryGetValue(el.Id, out var options) && options != null)
                {
                    foreach (var opt in options)
                    {
                        var option = new SurveyElementOption
                        {
                            Id = Guid.NewGuid(),
                            ElementId = newElementId,
                            Value = opt.Value,
                            DisplayText = opt.DisplayText,
                            SortOrder = opt.SortOrder,
                            IsDefault = opt.IsDefault,
                            IsActive = opt.IsActive
                        };

                        _optionRepository.Add(option);
                    }
                }
            }

            scope.Complete();

            int activeEmployeeCount = _employeeRepository.GetActiveEmployeeCount();

            return new SurveyDto
            {
                Id = newSurveyId,
                Code = newCode,
                Name = newName,
                Description = existingSurvey.Description,
                StartDate = existingSurvey.StartDate,
                EndDate = existingSurvey.EndDate,
                Status = 0,
                MaxAttempts = existingSurvey.MaxAttempts,
                CreatedDate = createdDate,
                UpdatedDate = null,
                Targets = targets.Select(t => new SurveyTargetDto { TargetType = t.TargetType, TargetId = t.TargetId }).ToList(),
                TotalResponses = activeEmployeeCount,
                CompletedCount = 0,
                IncompleteCount = activeEmployeeCount,
                CompletionRate = 0
            };
        }

        public void CloseSurvey(Guid id)
        {
            var existingSurvey = _repository.GetById(id);
            if (existingSurvey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            _repository.UpdateStatus(id, 2); // 2 = Closed
        }
    }
}
