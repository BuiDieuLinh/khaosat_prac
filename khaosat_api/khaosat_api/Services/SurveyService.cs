using System.Security.Cryptography;
using System.Text;
using System.Transactions;
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
        private readonly ISurveyTargetRepository _targetRepository;
        private readonly ISurveyParticipantRepository _participantRepository;
        private readonly ISurveyAccessRepository _accessRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public SurveyService(
            ISurveyRepository repository,
            ISurveyElementRepository elementRepository,
            ISurveyElementOptionRepository optionRepository,
            ISurveyResponseRepository responseRepository,
            ISurveyAnswerRepository answerRepository,
            IEmployeeRepository employeeRepository,
            ISurveyTargetRepository targetRepository,
            ISurveyParticipantRepository participantRepository,
            ISurveyAccessRepository accessRepository,
            IAuditLogRepository auditLogRepository)
        {
            _repository = repository;
            _elementRepository = elementRepository;
            _optionRepository = optionRepository;
            _responseRepository = responseRepository;
            _answerRepository = answerRepository;
            _employeeRepository = employeeRepository;
            _targetRepository = targetRepository;
            _participantRepository = participantRepository;
            _accessRepository = accessRepository;
            _auditLogRepository = auditLogRepository;
        }

        private void LogAudit(string action, string entityType, string entityId, string? oldValue, string? newValue, string? username, string? ipAddress, string? userAgent)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserName = username ?? "System",
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    OldValue = oldValue,
                    NewValue = newValue,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.Now
                };
                _auditLogRepository.Add(auditLog);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogAudit Exception: {ex.Message}");
            }
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
                if (t.TargetType == 2 && employee.DepartmentId.HasValue && t.DepartmentId == employee.DepartmentId.Value) return true;
                if (t.TargetType == 3 && employee.PositionId.HasValue && t.PositionId == employee.PositionId.Value) return true;
            }

            return false;
        }

        private (List<EmployeeResponse> Employees, string Summary) GetTargetEmployeesForSurvey(Guid surveyId)
        {
            var allEmployees = _employeeRepository.GetAll().Where(e => e.IsActive).ToList();
            var targets = _targetRepository.GetBySurveyId(surveyId);

            if (targets == null || targets.Count == 0 || targets.Any(t => t.TargetType == 1))
            {
                return (allEmployees, $"Toàn bộ công ty ({allEmployees.Count} nhân sự)");
            }

            var targetEmployees = new List<EmployeeResponse>();
            var deptTargets = targets.Where(t => t.TargetType == 2 && t.DepartmentId.HasValue).Select(t => t.DepartmentId!.Value).ToHashSet();
            var posTargets = targets.Where(t => t.TargetType == 3 && t.PositionId.HasValue).Select(t => t.PositionId!.Value).ToHashSet();

            foreach (var emp in allEmployees)
            {
                bool isMatch = false;

                if (emp.DepartmentId.HasValue && deptTargets.Contains(emp.DepartmentId.Value))
                {
                    isMatch = true;
                }
                else if (emp.PositionId.HasValue && posTargets.Contains(emp.PositionId.Value))
                {
                    isMatch = true;
                }

                if (isMatch)
                {
                    targetEmployees.Add(emp);
                }
            }

            string summary = "";
            if (deptTargets.Count > 0 && posTargets.Count > 0)
            {
                summary = $"{deptTargets.Count} phòng ban, {posTargets.Count} vị trí ({targetEmployees.Count} nhân sự)";
            }
            else if (deptTargets.Count > 0)
            {
                summary = $"{deptTargets.Count} phòng ban ({targetEmployees.Count} nhân sự)";
            }
            else if (posTargets.Count > 0)
            {
                summary = $"{posTargets.Count} vị trí ({targetEmployees.Count} nhân sự)";
            }
            else
            {
                summary = $"{targetEmployees.Count} nhân sự mục tiêu";
            }

            return (targetEmployees, summary);
        }

        public List<SurveyDto> GetSurveys(Guid? currentUserId = null)
        {
            var surveys = _repository.GetAll();
            var completedParticipantCounts = _participantRepository.GetCompletedCounts();
            var totalResponseCounts = _responseRepository.GetCompletedCounts();

            EmployeeResponse? employee = null;
            if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
            {
                employee = _employeeRepository.GetByIdAsync(currentUserId.Value).GetAwaiter().GetResult();
            }

            var resultList = new List<SurveyDto>();

            foreach (var x in surveys)
            {
                if (employee != null && x.AccessType == 1 && !IsEmployeeInTargetAudience(x.Id, employee))
                {
                    continue; // Skip internal survey if user is not in target audience
                }

                var targets = _targetRepository.GetBySurveyId(x.Id);
                var status = x.Status;
                if (x.Status == SurveyStatus.Active && x.EndDate.HasValue && x.EndDate.Value < DateTime.Now)
                {
                    status = 0;
                    _repository.UpdateStatus(x.Id, 0);
                }

                int targetCount = GetTargetEmployeesForSurvey(x.Id).Employees.Count;
                int completedCount = (x.AccessType == 1 || x.AccessType == 3)
                    ? (completedParticipantCounts.TryGetValue(x.Id, out int cCount) ? cCount : 0)
                    : (totalResponseCounts.TryGetValue(x.Id, out int rCount) ? rCount : 0);

                int incompleteCount = targetCount > 0 ? Math.Max(0, targetCount - completedCount) : 0;
                double completionRate = targetCount > 0 ? Math.Round((double)completedCount / targetCount * 100, 2) : 0;

                resultList.Add(new SurveyDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = (byte)status,
                    MaxAttempts = x.MaxAttempts,
                    AccessType = x.AccessType,
                    AnonymousMode = x.AnonymousMode,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate,
                    Targets = targets.Select(t => new SurveyTargetDto { TargetType = t.TargetType, DepartmentId = t.DepartmentId, PositionId = t.PositionId }).ToList(),
                    TotalResponses = targetCount > 0 ? targetCount : completedCount,
                    CompletedCount = completedCount,
                    IncompleteCount = incompleteCount,
                    CompletionRate = completionRate
                });
            }

            return resultList;
        }

        public PagedResult<SurveyDto> GetSurveys(SurveyFilterDto filter, Guid? currentUserId = null, bool isAdminOrManager = false)
        {
            var targetUserId = isAdminOrManager ? null : currentUserId;
            var pagedResult = _repository.GetSurveys(filter, targetUserId);

            var completedParticipantCounts = _participantRepository.GetCompletedCounts();
            var totalResponseCounts = _responseRepository.GetCompletedCounts();

            var resultList = new List<SurveyDto>();

            foreach (var survey in pagedResult.Data)
            {
                var status = survey.Status;

                if (status == SurveyStatus.Active && survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now)
                {
                    status = SurveyStatus.Closed;
                    _repository.UpdateStatus(survey.Id, (byte)SurveyStatus.Closed);
                }

                var targets = _targetRepository.GetBySurveyId(survey.Id);

                int targetCount = GetTargetEmployeesForSurvey(survey.Id).Employees.Count;
                int completedCount = (survey.AccessType == 1 || survey.AccessType == 3)
                    ? (completedParticipantCounts.TryGetValue(survey.Id, out int cCount) ? cCount : 0)
                    : (totalResponseCounts.TryGetValue(survey.Id, out int rCount) ? rCount : 0);

                int incompleteCount = targetCount > 0 ? Math.Max(0, targetCount - completedCount) : 0;

                double completionRate = targetCount > 0
                    ? Math.Round((double)completedCount / targetCount * 100, 2)
                    : 0;

                resultList.Add(new SurveyDto
                {
                    Id = survey.Id,
                    Code = survey.Code,
                    Name = survey.Name,
                    Description = survey.Description,
                    StartDate = survey.StartDate,
                    EndDate = survey.EndDate,
                    Status = (byte)status,
                    MaxAttempts = survey.MaxAttempts,
                    AccessType = survey.AccessType,
                    AnonymousMode = survey.AnonymousMode,
                    CreatedDate = survey.CreatedDate,
                    UpdatedDate = survey.UpdatedDate,
                    Targets = targets.Select(t => new SurveyTargetDto
                    {
                        TargetType = t.TargetType,
                        DepartmentId = t.DepartmentId,
                        PositionId = t.PositionId
                    }).ToList(),
                    TotalResponses = targetCount > 0 ? targetCount : completedCount,
                    CompletedCount = completedCount,
                    IncompleteCount = incompleteCount,
                    CompletionRate = completionRate
                });
            }

            return new PagedResult<SurveyDto>(
                resultList,
                pagedResult.TotalCount,
                pagedResult.PageNumber,
                pagedResult.PageSize);
        }

        public SurveyDetailDto? GetSurveyDetail(Guid id, Guid? currentUserId = null)
        {
            var survey = _repository.GetById(id);
            if (survey == null)
            {
                return null;
            }

            if (survey.AccessType == 1 && currentUserId.HasValue && currentUserId.Value != Guid.Empty)
            {
                var employee = _employeeRepository.GetByIdAsync(currentUserId.Value).GetAwaiter().GetResult();
                if (employee != null && !IsEmployeeInTargetAudience(id, employee))
                {
                    throw new InvalidOperationException("NOT_IN_TARGET_AUDIENCE");
                }
            }

            var status = survey.Status;
            if (survey.Status == SurveyStatus.Active && survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now)
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
                Status = (byte)status,
                MaxAttempts = survey.MaxAttempts,
                AccessType = survey.AccessType,
                AnonymousMode = survey.AnonymousMode,
                CreatedDate = survey.CreatedDate,
                UpdatedDate = survey.UpdatedDate,
                Targets = targets.Select(t => new SurveyTargetDto { TargetType = t.TargetType, DepartmentId = t.DepartmentId, PositionId = t.PositionId }).ToList(),
                Elements = elementDetails
            };
        }

        public SurveyDetailDto? GetPublicSurveyDetail(string token)
        {
            var access = _accessRepository.GetByTokenHash(token);
            Survey? survey = null;
            if (access != null)
            {
                survey = _repository.GetById(access.SurveyId);
            }
            else if (Guid.TryParse(token, out Guid surveyGuid))
            {
                survey = _repository.GetById(surveyGuid);
            }

            if (survey == null || (survey.AccessType != 2 && access == null))
            {
                throw new InvalidOperationException("Khảo sát công khai không tồn tại hoặc không có quyền truy cập.");
            }

            if (survey.Status != SurveyStatus.Active || (survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now))
            {
                throw new InvalidOperationException("Cuộc khảo sát đã kết thúc hoặc chưa phát hành.");
            }

            return GetSurveyDetail(survey.Id);
        }

        public void SubmitSurvey(SurveySubmitDto dto, string? username = null, string? ipAddress = null, string? userAgent = null)
        {
            var survey = _repository.GetById(dto.SurveyId);
            if (survey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            if (survey.Status != SurveyStatus.Active || (survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now))
            {
                throw new InvalidOperationException("Cuộc khảo sát đã kết thúc hoặc chưa phát hành.");
            }

            if (survey.AccessType == 1) // Internal
            {
                if (!dto.EmployeeId.HasValue || dto.EmployeeId.Value == Guid.Empty)
                {
                    throw new InvalidOperationException("Vui lòng đăng nhập để thực hiện khảo sát nội bộ.");
                }

                var employee = _employeeRepository.GetByIdAsync(dto.EmployeeId.Value).GetAwaiter().GetResult();
                if (employee != null && !IsEmployeeInTargetAudience(dto.SurveyId, employee))
                {
                    throw new InvalidOperationException("NOT_IN_TARGET_AUDIENCE");
                }

                if (survey.MaxAttempts.HasValue && survey.MaxAttempts.Value > 0)
                {
                    var userAttemptCount = _participantRepository.GetCountBySurveyAndEmployee(dto.SurveyId, dto.EmployeeId.Value);
                    if (userAttemptCount >= survey.MaxAttempts.Value)
                    {
                        throw new InvalidOperationException("MAX_ATTEMPTS_EXCEEDED");
                    }
                }

                var participant = new SurveyParticipant
                {
                    Id = Guid.NewGuid(),
                    SurveyId = dto.SurveyId,
                    EmployeeId = dto.EmployeeId.Value,
                    Status = 1,
                    SubmittedAt = DateTime.Now
                };
                _participantRepository.Add(participant);
            }

            var responseId = Guid.NewGuid();
            var response = new SurveyResponse
            {
                Id = responseId,
                SurveyId = dto.SurveyId,
                // CRITICAL Privacy Rule: If AnonymousMode is true, set EmployeeId = null!
                EmployeeId = survey.AnonymousMode ? null : dto.EmployeeId,
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

            LogAudit("SUBMIT_SURVEY", "Survey", dto.SurveyId.ToString(), null, $"AnonymousMode={survey.AnonymousMode}", username, ipAddress, userAgent);
        }

        public void SubmitPublicSurvey(SurveySubmitDto dto, string? ipAddress = null, string? userAgent = null)
        {
            var survey = _repository.GetById(dto.SurveyId);
            if (survey == null)
            {
                throw new InvalidOperationException("Khảo sát công khai không tồn tại.");
            }

            if (survey.AccessType != 2 && survey.AccessType != 3)
            {
                throw new InvalidOperationException("Khảo sát này không hỗ trợ nộp công khai.");
            }

            if (survey.Status != SurveyStatus.Active || (survey.EndDate.HasValue && survey.EndDate.Value < DateTime.Now))
            {
                throw new InvalidOperationException("Cuộc khảo sát đã kết thúc hoặc chưa phát hành.");
            }

            // Public survey: EmployeeId is NULL
            dto.EmployeeId = null;
            SubmitSurvey(dto, "PublicUser", ipAddress, userAgent);
        }

        public void CreateNested(SurveyCreateNestedDto dto)
        {
            using var scope = new TransactionScope();

            var surveyId = Guid.NewGuid();
            var survey = new Survey
            {
                Id = surveyId,
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                MaxAttempts = dto.MaxAttempts,
                AccessType = dto.AccessType,
                AnonymousMode = dto.AnonymousMode,
                CreatedDate = DateTime.Now,
                UpdatedDate = null
            };

            _repository.Add(survey);

            // Also Update SurveyAccess record for Public access if AccessType == 2
            if (dto.AccessType == 2)
            {
                var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(surveyId.ToString() + DateTime.Now.Ticks)));
                _accessRepository.Add(new SurveyAccess
                {
                    Id = Guid.NewGuid(),
                    SurveyId = surveyId,
                    AccessType = dto.AccessType,
                    TokenHash = tokenHash.Substring(0, 16).ToLower(),
                    IsActive = true,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate
                });
            }

            if (dto.Targets != null && dto.Targets.Count > 0)
            {
                foreach (var tDto in dto.Targets)
                {
                    var target = new SurveyTarget
                    {
                        Id = Guid.NewGuid(),
                        SurveyId = surveyId,
                        TargetType = tDto.TargetType,
                        DepartmentId = tDto.TargetType == 1 ? null : tDto.DepartmentId,
                        PositionId = tDto.TargetType == 1 ? null : tDto.PositionId
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

        public void UpdateNested(Guid id, SurveyUpdateNestedDto dto)
        {
            var existingSurvey = _repository.GetById(id);
            if (existingSurvey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            if (existingSurvey.Status == SurveyStatus.Closed)
            {
                throw new InvalidOperationException("Khảo sát đã được đóng.");
            }

            var responseCount = _responseRepository.GetCountBySurveyId(id);
            if (responseCount > 0)
            {
                throw new InvalidOperationException("Khảo sát đã có người tham gia.");
            }

            if (existingSurvey.Status == SurveyStatus.Active && existingSurvey.StartDate.HasValue && existingSurvey.StartDate.Value <= DateTime.Now)
            {
                throw new InvalidOperationException("Khảo sát đã bắt đầu.");
            }

            using var scope = new TransactionScope();

            existingSurvey.Name = dto.Name;
            existingSurvey.Description = dto.Description;
            existingSurvey.StartDate = dto.StartDate;
            existingSurvey.EndDate = dto.EndDate;
            existingSurvey.Status = dto.Status;
            existingSurvey.MaxAttempts = dto.MaxAttempts;
            existingSurvey.AccessType = dto.AccessType;
            existingSurvey.AnonymousMode = dto.AnonymousMode;
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
                        DepartmentId = tDto.TargetType == 1 ? null : tDto.DepartmentId,
                        PositionId = tDto.TargetType == 1 ? null : tDto.PositionId
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

            int activeEmployeeCount = _employeeRepository.GetActiveEmployeeCount();
            var newSurveyId = Guid.NewGuid();
            var newName = GenerateCloneName(existingSurvey.Name);
            var newCode = GenerateUniqueCode();
            var createdDate = DateTime.Now;

            using (var scope = new TransactionScope())
            {
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
                    AccessType = existingSurvey.AccessType,
                    AnonymousMode = existingSurvey.AnonymousMode,
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
                            DepartmentId = t.DepartmentId,
                            PositionId = t.PositionId
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
            }

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
                Targets = targets.Select(t => new SurveyTargetDto { TargetType = t.TargetType, DepartmentId = t.DepartmentId, PositionId = t.PositionId }).ToList(),
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

        public void ChangeAccessType(Guid id, int accessType)
        {
            var existingSurvey = _repository.GetById(id);
            if (existingSurvey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            _repository.UpdateAccessType(id, accessType);

            // If changing to public, ensure SurveyAccess record exists
            if (accessType == 2)
            {
                var existingAccess = _accessRepository.GetBySurveyId(id);
                if (existingAccess == null)
                {
                    var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(id.ToString() + DateTime.Now.Ticks))).Substring(0, 16).ToLower();
                    _accessRepository.Add(new SurveyAccess
                    {
                        Id = Guid.NewGuid(),
                        SurveyId = id,
                        AccessType = accessType,
                        TokenHash = tokenHash,
                        IsActive = true
                    });
                }
            }
        }

        public void ChangeAnonymousMode(Guid id, bool anonymousMode)
        {
            var existingSurvey = _repository.GetById(id);
            if (existingSurvey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            _repository.UpdateAnonymousMode(id, anonymousMode);
        }

        public SurveyReportDto GetSurveyReport(Guid id)
        {
            var survey = _repository.GetById(id);
            if (survey == null)
            {
                throw new InvalidOperationException("Khảo sát không tồn tại.");
            }

            int totalResponses = _responseRepository.GetCountBySurveyId(id);
            const int MIN_THRESHOLD = 5;
            bool isThresholdMet = totalResponses >= MIN_THRESHOLD;

            var (targetEmployees, targetSummary) = GetTargetEmployeesForSurvey(survey.Id);
            int totalTargetParticipants = targetEmployees.Count;

            var participants = _participantRepository.GetBySurveyId(id);
            var completedEmployeeIds = participants.Where(p => p.Status == 1 && p.EmployeeId.HasValue).Select(p => p.EmployeeId!.Value).ToHashSet();

            int completedParticipantsCount = targetEmployees.Count(e => completedEmployeeIds.Contains(e.Id));

            double completionRate = totalTargetParticipants > 0
                ? Math.Round((double)completedParticipantsCount / totalTargetParticipants * 100, 1)
                : 0;

            var departmentBreakdown = new List<DepartmentReportDto>();
            var deptGroups = targetEmployees.GroupBy(e => new
            {
                e.DepartmentId,
                DepartmentName = string.IsNullOrWhiteSpace(e.DepartmentName) ? "Chưa phân phòng ban" : e.DepartmentName
            });

            foreach (var group in deptGroups.OrderBy(g => g.Key.DepartmentName))
            {
                int assignedCount = group.Count();
                int deptCompletedCount = group.Count(e => completedEmployeeIds.Contains(e.Id));
                double deptCompletionRate = assignedCount > 0
                    ? Math.Round((double)deptCompletedCount / assignedCount * 100, 1)
                    : 0;

                departmentBreakdown.Add(new DepartmentReportDto
                {
                    DepartmentId = group.Key.DepartmentId,
                    DepartmentName = group.Key.DepartmentName,
                    TotalAssigned = assignedCount,
                    CompletedCount = deptCompletedCount,
                    CompletionRate = deptCompletionRate
                });
            }

            var report = new SurveyReportDto
            {
                SurveyId = survey.Id,
                SurveyCode = survey.Code,
                SurveyName = survey.Name,
                Description = survey.Description,
                Status = (byte)survey.Status,
                AccessType = survey.AccessType,
                AnonymousMode = survey.AnonymousMode,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                CreatedDate = survey.CreatedDate,
                TotalResponses = totalResponses,
                TotalTargetParticipants = totalTargetParticipants,
                CompletedParticipantsCount = completedParticipantsCount,
                CompletionRate = completionRate,
                TargetSummary = targetSummary,
                DepartmentBreakdown = departmentBreakdown
            };

            // 1. Group Breakdown for compatibility
            report.GroupBreakdown.Add(new GroupReportItemDto
            {
                GroupName = survey.AccessType == 1 ? "Nhân sự nội bộ thuộc đối tượng" : "Người tham gia khảo sát",
                ResponseCount = totalResponses,
                IsPrivacyThresholdMet = isThresholdMet,
                Note = isThresholdMet ? "Đạt ngưỡng bảo vệ quyền riêng tư" : "Dưới ngưỡng bảo vệ (dưới 5 phản hồi)"
            });

            // 2. Questions / Elements Breakdown
            var elements = _elementRepository.GetBySurveyId(id).OrderBy(e => e.SortOrder).ToList();
            var allAnswers = _answerRepository.GetBySurveyId(id);

            foreach (var element in elements)
            {
                string caption = element.FieldName;
                string dataType = "Textbox";

                if (!string.IsNullOrWhiteSpace(element.ConfigType))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(element.ConfigType);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("Caption", out var capProp) && !string.IsNullOrWhiteSpace(capProp.GetString()))
                        {
                            caption = capProp.GetString()!;
                        }
                        if (root.TryGetProperty("DataType", out var dtProp))
                        {
                            dataType = dtProp.GetString() ?? "Textbox";
                        }
                    }
                    catch { }
                }

                var elementAnswers = allAnswers.Where(a => a.ElementId == element.Id).ToList();
                var options = _optionRepository.GetByElementId(element.Id).OrderBy(o => o.SortOrder).ToList();

                var qReport = new QuestionReportDto
                {
                    ElementId = element.Id,
                    FieldName = element.FieldName,
                    Caption = caption,
                    DataType = dataType,
                    SortOrder = element.SortOrder,
                    TotalAnswerCount = elementAnswers.Count
                };

                if (options.Count > 0)
                {
                    foreach (var opt in options)
                    {
                        int count = elementAnswers.Count(a => a.OptionId == opt.Id || (!string.IsNullOrEmpty(a.Value) && a.Value.Trim() == opt.Value.Trim()));
                        double percentage = elementAnswers.Count > 0
                            ? Math.Round((double)count / elementAnswers.Count * 100, 1)
                            : 0;

                        qReport.Options.Add(new OptionReportDto
                        {
                            OptionId = opt.Id,
                            DisplayText = opt.DisplayText,
                            Value = opt.Value,
                            Count = count,
                            Percentage = percentage
                        });
                    }
                }
                else
                {
                    // Text / open-ended questions
                    if (isThresholdMet || !survey.AnonymousMode)
                    {
                        qReport.TextAnswers = elementAnswers
                            .Where(a => !string.IsNullOrWhiteSpace(a.Value))
                            .Select(a => a.Value!.Trim())
                            .ToList();
                    }
                }

                report.Questions.Add(qReport);
            }

            return report;
        }

        public PagedResult<AuditLogDto> GetAuditLogs(int pageNumber, int pageSize, string? actionFilter = null, string? searchKeyword = null)
        {
            var logs = _auditLogRepository.GetLogs(pageNumber, pageSize, actionFilter, searchKeyword);
            var dtos = logs.Data.Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserName = l.UserName,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                OldValue = l.OldValue,
                NewValue = l.NewValue,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                CreatedAt = l.CreatedAt
            }).ToList();

            return new PagedResult<AuditLogDto>(dtos, logs.TotalCount, logs.PageNumber, logs.PageSize);
        }
    }
}
