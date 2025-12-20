using CustomerAgreements.Data;
using CustomerAgreements.Contracts.Dtos;
using CustomerAgreements.Models;
using CustomerAgreements.Services;
using CustomerAgreements.Options;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net.Http.Json;


namespace CustomerAgreements.Pages.Agreements
{
    public class CreateModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly AgreementResponseService _agreementService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FeatureFlagsOptions _flags;

        public CreateModel(ILogger<CreateModel> logger, ApplicationDbContext context, AgreementResponseService agreementService, IHttpClientFactory httpClientFactory,
            IOptions<FeatureFlagsOptions> flags)
        {
            _logger = logger;
            _context = context;
            _agreementService = agreementService;
            _httpClientFactory = httpClientFactory;
            _flags = flags.Value;
        }

        [BindProperty]
        public CustomerAgreements.Models.Agreement Agreement { get; set; } = new CustomerAgreements.Models.Agreement();
        [BindProperty]
        public AgreementFormViewModel FormModel { get; set; } = new();        

        public string? Acknowledgement { get; set; }
        public string? Instructions { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Preview { get; set; }

        [BindProperty]
        public string ActionType { get; set; } = "";

        [TempData]
        public string? StatusMessage { get; set; }


        public async Task<IActionResult> OnGetAsync(int questionnaireId)
        {
            try
            {
                await LoadQuestionnaireAsync(questionnaireId);

                // Initialize a new Customer record
                FormModel.Customer = new CustomerAgreements.Models.Customer();

                // Initialize a new Agreement record
                FormModel.Agreement = new CustomerAgreements.Models.Agreement
                {
                    QuestionnaireID = questionnaireId
                };                

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading agreement page. {ex.Message}",
                    Agreement.AgreementID,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(int questionnaireId)
        {
            var isSubmit = ActionType == "Submit";

            await LoadQuestionnaireAsync(questionnaireId);

            // Keep your existing "Draft skips questionnaire validation" behavior
            if (!isSubmit)
            {
                var keysToKeep = new[]
                {
                    "Customer.CompanyName",
                    "Customer.ContactName",
                    "Customer.EmailAddress"
                };

                var keysToRemove = ModelState.Keys
                    .Where(k => !keysToKeep.Any(keep => k.Contains(keep)))
                    .ToList();

                foreach (var key in keysToRemove)
                    ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
                return Page();

            try
            {
                if (_flags.UseApiForAgreementSave)
                {
                    // 1) Try API first
                    var apiResult = await TrySaveViaApiAsync(questionnaireId, isSubmit);

                    if (apiResult != null)
                    {
                        // Redirect using API-created AgreementId
                        if (isSubmit)
                        {
                            return RedirectToPage("/Agreement/Detail",
                                new { questionnaireId, agreementId = apiResult.AgreementId });
                        }

                        StatusMessage = "Draft saved successfully.";
                        return RedirectToPage("/Agreement/Edit",
                            new { questionnaireId, agreementId = apiResult.AgreementId });
                    }

                    // API failed but didn't throw - only fallback if allowed
                    if (!_flags.AllowDbFallback)
                    {
                        ModelState.AddModelError("", "Unable to save at this time. Please try again.");
                        return Page();
                    }
                }

                // Either API is off OR fallback allowed and needed
                return await SaveViaDbFallbackAsync(questionnaireId, isSubmit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving agreement (API + fallback failed). QuestionnaireId={QuestionnaireId}", questionnaireId);

                ModelState.AddModelError("", "Unable to save agreement. See logs for details.");
                return Page();
            }
        }

        private async Task<CreateAgreementResponseDto?> TrySaveViaApiAsync(int questionnaireId, bool isSubmit)
        {
            try
            {
                var requestDto = BuildCreateAgreementRequestDto(questionnaireId, isSubmit);

                var client = _httpClientFactory.CreateClient("CustomerAgreementsApi");

                var response = await client.PostAsJsonAsync("api/agreements", requestDto);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "API save failed. HttpStatus={HttpStatus}. ActionType={ActionType}. Body={Body}",
                        (int)response.StatusCode,
                        isSubmit ? "Submit" : "Draft",
                        body);

                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<CreateAgreementResponseDto>();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "API save threw exception; will fall back to DB save.");
                return null;
            }
        }

        private CreateAgreementRequestDto BuildCreateAgreementRequestDto(int questionnaireId, bool isSubmit)
        {
            // Build answers from the form using the questionnaire definition you already loaded
            var answers = new List<AnswerDto>();

            if (FormModel?.Questionnaire?.Sections != null)
            {
                foreach (var section in FormModel.Questionnaire.Sections)
                {
                    if (section.Questions == null) continue;

                    foreach (var question in section.Questions)
                    {
                        var key = $"question_{question.QuestionID}";

                        // Single checkbox: if missing, treat as false
                        if (question.AnswerType == "Single Checkbox")
                        {
                            var isChecked = Request.Form.ContainsKey(key);
                            answers.Add(new AnswerDto
                            {
                                QuestionId = question.QuestionID,
                                Value = isChecked ? "true" : "false"
                            });
                            continue;
                        }

                        // Date / text / single value
                        if (!question.AnswerType.Contains("List", StringComparison.OrdinalIgnoreCase))
                        {
                            var value = Request.Form[key].ToString();
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                answers.Add(new AnswerDto
                                {
                                    QuestionId = question.QuestionID,
                                    Value = value
                                });
                            }
                            continue;
                        }

                        // List types
                        if (question.AnswerType.Contains("Checkbox", StringComparison.OrdinalIgnoreCase))
                        {
                            // multiple selections
                            var selected = Request.Form[key].ToArray();
                            if (selected.Length == 0) continue;

                            // For each selected value, attach dependent answers for THAT list item (if any)
                            foreach (var selectedValue in selected)
                            {
                                var listItem = question.QuestionLists?.FirstOrDefault(ql => ql.ListValue == selectedValue);

                                var depAnswers = new List<DependentAnswerDto>();
                                if (listItem?.Conditional == true && listItem.DependentQuestions != null)
                                {
                                    foreach (var dep in listItem.DependentQuestions)
                                    {
                                        var depKey = $"dependentQuestion_{dep.DependentQuestionID}";
                                        var depVal = Request.Form[depKey].ToString();

                                        depAnswers.Add(new DependentAnswerDto
                                        {
                                            DependentQuestionId = dep.DependentQuestionID,
                                            Value = depVal
                                        });
                                    }
                                }

                                answers.Add(new AnswerDto
                                {
                                    QuestionId = question.QuestionID,
                                    Value = selectedValue,              // API resolves by ListValue
                                    DependentAnswers = depAnswers.Count > 0 ? depAnswers : null
                                });
                            }

                            continue;
                        }

                        // Radio / dropdown: single selection
                        {
                            var selectedValue = Request.Form[key].ToString();
                            if (string.IsNullOrWhiteSpace(selectedValue)) continue;

                            var listItem = question.QuestionLists?.FirstOrDefault(ql => ql.ListValue == selectedValue);

                            var depAnswers = new List<DependentAnswerDto>();
                            if (listItem?.Conditional == true && listItem.DependentQuestions != null)
                            {
                                foreach (var dep in listItem.DependentQuestions)
                                {
                                    var depKey = $"dependentQuestion_{dep.DependentQuestionID}";
                                    var depVal = Request.Form[depKey].ToString();

                                    depAnswers.Add(new DependentAnswerDto
                                    {
                                        DependentQuestionId = dep.DependentQuestionID,
                                        Value = depVal
                                    });
                                }
                            }

                            answers.Add(new AnswerDto
                            {
                                QuestionId = question.QuestionID,
                                Value = selectedValue,              // API resolves by ListValue
                                DependentAnswers = depAnswers.Count > 0 ? depAnswers : null
                            });
                        }
                    }
                }
            }

            return new CreateAgreementRequestDto
            {
                QuestionnaireId = questionnaireId,
                ActionType = isSubmit ? "Submit" : "Draft",
                Customer = new CustomerDto
                {
                    CompanyName = FormModel.Customer.CompanyName,
                    ContactName = FormModel.Customer.ContactName,
                    EmailAddress = FormModel.Customer.EmailAddress
                },
                Answers = answers
            };
        }

        private async Task<IActionResult> SaveViaDbFallbackAsync(int questionnaireId, bool isSubmit)
        {
            // === Your original DB save logic, unchanged ===
            _context.Customers.Add(FormModel.Customer);
            await _context.SaveChangesAsync();

            FormModel.Agreement.CustomerID = FormModel.Customer.CustomerID;
            FormModel.Agreement.QuestionnaireID = questionnaireId;
            FormModel.Agreement.CustomerName = FormModel.Customer.CompanyName;
            FormModel.Agreement.CustomerEmail = FormModel.Customer.EmailAddress;
            FormModel.Agreement.Status = isSubmit ? "Submitted" : "Draft";

            if (isSubmit)
                FormModel.Agreement.SubmittedDate = DateTime.UtcNow;

            _context.Agreements.Add(FormModel.Agreement);
            await _context.SaveChangesAsync();

            await _agreementService.SaveOrUpdateAnswersFromFormAsync(
                questionnaireId, FormModel.Agreement, Request.Form, FormModel.Questionnaire);

            if (isSubmit)
            {
                return RedirectToPage("/Agreement/Detail",
                    new { questionnaireId, agreementId = FormModel.Agreement.AgreementID });
            }

            StatusMessage = "Draft saved successfully.";
            return RedirectToPage("/Agreement/Edit",
                new { questionnaireId, agreementId = FormModel.Agreement.AgreementID });
        }


        private async Task LoadQuestionnaireAsync(int questionnaireId)
        {
            if (_flags.UseApiForAgreementLoad)
            {
                try
                {
                    await LoadQuestionnaireFromApiAsync(questionnaireId);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "API load failed for questionnaireId {QuestionnaireId}", questionnaireId);

                    if (!_flags.AllowDbFallback)
                        throw; // or show friendly error, your choice
                }
            }

            await LoadQuestionnaireFromDbAsync(questionnaireId);
        }

        private async Task LoadQuestionnaireFromDbAsync(int questionnaireId)
        {
            FormModel.Questionnaire = await _context.Questionnaires
                    .Include(q => q.Sections)
                        .ThenInclude(s => s.Questions)
                        .ThenInclude(ql => ql.QuestionLists)
                        .ThenInclude(dq => dq.DependentQuestions)
                    .FirstOrDefaultAsync(q => q.QuestionnaireID == questionnaireId)
                    ?? new Questionnaire();

            var instructions = await _context.Notifications
                .Where(n => n.NotificationKey == "Instructions")
                .FirstOrDefaultAsync();

            Instructions = instructions?.Text ?? "No instructions available.";

            var acknowledgement = await _context.Notifications
            .Where(n => n.NotificationKey == "Acknowledgement")
            .FirstOrDefaultAsync();

            Acknowledgement = acknowledgement?.Text ?? "No acknowledgement available.";
                        
            _logger.LogInformation(
                "Loaded questionnaire {QuestionnaireId} via DB fallback.",
                questionnaireId);
        }

        private async Task LoadQuestionnaireFromApiAsync(int questionnaireId)
        {
            var client = _httpClientFactory.CreateClient("CustomerAgreementsApi");

            var response = await client.GetAsync($"api/questionnaires/{questionnaireId}/create-payload");
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<CreateAgreementPayloadDto>();

            if (payload == null)
            {
                FormModel.Questionnaire = new Questionnaire();
                Instructions = "No instructions available.";
                Acknowledgement = "No acknowledgement available.";
                return;
            }

            Instructions = payload.Instructions;
            Acknowledgement = payload.Acknowledgement;

            FormModel.Questionnaire = new Questionnaire
            {
                QuestionnaireID = payload.Questionnaire.QuestionnaireId,
                QuestionnaireName = payload.Questionnaire.QuestionnaireName,
                Status = payload.Questionnaire.Status,
                Sections = payload.Questionnaire.Sections
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new Section
                    {
                        SectionID = s.SectionId,
                        QuestionnaireID = s.QuestionnaireId,
                        Text = s.Text,
                        SortOrder = s.SortOrder,
                        IncludeInstructions = s.IncludeInstructions,
                        Instructions = s.Instructions,
                        Questions = s.Questions
                            .OrderBy(q => q.SortOrder)
                            .Select(q => new Question
                            {
                                ID = q.Id,
                                QuestionID = q.QuestionId,
                                QuestionnaireID = q.QuestionnaireId,
                                SectionID = q.SectionId,
                                QuestionTitle = q.QuestionTitle,
                                Text = q.Text,
                                QuestionText = q.QuestionText,
                                AnswerType = q.AnswerType,
                                IsRequired = q.IsRequired,
                                SortOrder = q.SortOrder,
                                QuestionLists = q.QuestionLists
                                    .OrderBy(ql => ql.SortOrder)
                                    .Select(ql => new QuestionList
                                    {
                                        QuestionListID = ql.QuestionListId,
                                        QuestionnaireID = ql.QuestionnaireId,
                                        SectionID = ql.SectionId,
                                        QuestionID = ql.QuestionId,
                                        ListValue = ql.ListValue,
                                        Conditional = ql.Conditional,
                                        SortOrder = ql.SortOrder,
                                        SendEmail = ql.SendEmail,
                                        NotificationID = ql.NotificationId,
                                        DependentQuestions = ql.DependentQuestions
                                            .OrderBy(dq => dq.SortOrder)
                                            .Select(dq => new DependentQuestion
                                            {
                                                DependentQuestionID = dq.DependentQuestionId,
                                                QuestionnaireID = dq.QuestionnaireId,
                                                SectionID = dq.SectionId,
                                                QuestionID = dq.QuestionId,
                                                QuestionListID = dq.QuestionListId,
                                                DependentQuestionTitle = dq.DependentQuestionTitle,
                                                Text = dq.Text,
                                                DependentQuestionText = dq.DependentQuestionText,
                                                DependentAnswerType = dq.DependentAnswerType,
                                                IsRequired = dq.IsRequired,
                                                SortOrder = dq.SortOrder
                                            })
                                            .ToList()
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            };

            _logger.LogInformation(
                "Loaded questionnaire {QuestionnaireId} via API.",
                questionnaireId);



        }

    }
}
