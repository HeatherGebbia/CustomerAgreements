using CustomerAgreements.Contracts.Dtos;
using CustomerAgreements.Data;
using CustomerAgreements.Models;
using CustomerAgreements.Options;
using CustomerAgreements.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerAgreements.Pages.Agreements
{
    public class EditModel : PageModel
    {
        private readonly ILogger<EditModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly AgreementResponseService _agreementService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FeatureFlagsOptions _flags;

        public EditModel(ILogger<EditModel> logger, ApplicationDbContext context, AgreementResponseService agreementService, IHttpClientFactory httpClientFactory,
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

        [BindProperty]
        public string ActionType { get; set; } = "";

        [TempData]
        public string? StatusMessage { get; set; }


        public async Task<IActionResult> OnGetAsync(int questionnaireId, int agreementId)
        {
            try
            {
                await LoadQuestionnaireAsync(questionnaireId, agreementId);
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

        public async Task<IActionResult> OnPostAsync(int questionnaireId, int agreementId)
        {
            bool postedReadPrivacy = Agreement.ReadPrivacyPolicy;

            // Determine which button was clicked
            var isSubmit = ActionType == "Submit";

            await LoadQuestionnaireAsync(questionnaireId, agreementId);

            if (!isSubmit)
            {
                var keysToKeep = new[]
                {
                    "Customer.CompanyName",
                    "Customer.ContactName",
                    "Customer.EmailAddress"
                };

                // Remove validation errors for questionnaire questions
                var keysToRemove = ModelState.Keys
                    .Where(k => !keysToKeep.Any(keep => k.Contains(keep)))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    ModelState.Remove(key);
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Load the existing Agreement from the DB
                var existingAgreement = await _context.Agreements
                    .Include(a => a.Customer)
                    .FirstOrDefaultAsync(a => a.AgreementID == agreementId);

                if (existingAgreement == null)
                {
                    ModelState.AddModelError("", "Agreement not found.");
                    return Page();
                }

                // Update basic fields (these may be changed by user or system)
                existingAgreement.Status = isSubmit ? "Submitted" : "Draft";
                existingAgreement.ReadPrivacyPolicy = postedReadPrivacy;

                if (isSubmit)
                {
                    existingAgreement.SubmittedDate = DateTime.UtcNow;
                }

                // Save customer edits if applicable
                if (existingAgreement.Customer != null && FormModel.Customer != null)
                {
                    existingAgreement.Customer.CompanyName = FormModel.Customer.CompanyName;
                    existingAgreement.Customer.ContactName = FormModel.Customer.ContactName;
                    existingAgreement.Customer.EmailAddress = FormModel.Customer.EmailAddress;
                }

                await _context.SaveChangesAsync();                

                await _agreementService.SaveOrUpdateAnswersFromFormAsync(
                   questionnaireId, existingAgreement, Request.Form, FormModel.Questionnaire);

                if (isSubmit)
                {
                    return RedirectToPage("/Agreement/Detail", new { questionnaireId, agreementId = FormModel.Agreement.AgreementID });
                }
                else
                {
                    StatusMessage = "Draft saved successfully.";
                    return RedirectToPage("/Agreement/Edit", new { questionnaireId, agreementId = FormModel.Agreement.AgreementID });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving agreement. {ex.Message}",
                    Agreement.AgreementID,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to save agreement. See logs for details.");
                return Page();
            }
        }

        private async Task LoadQuestionnaireAsync(int questionnaireId, int agreementId)
        {
            if (_flags.UseApiForAgreementLoad)
            {
                try
                {
                    await LoadQuestionnaireFromApiAsync(questionnaireId, agreementId);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "API load failed for agreementId {AgreementId}", agreementId);

                    if (!_flags.AllowDbFallback)
                        throw; 
                }
            }

            await LoadQuestionnaireFromDbAsync(questionnaireId, agreementId);
        }

        private async Task LoadQuestionnaireFromDbAsync(int questionnaireId, int agreementId)
        {
            FormModel.Questionnaire = await _context.Questionnaires
                    .Include(q => q.Sections)
                        .ThenInclude(s => s.Questions)
                        .ThenInclude(ql => ql.QuestionLists)
                        .ThenInclude(dq => dq.DependentQuestions)
                    .FirstOrDefaultAsync(q => q.QuestionnaireID == questionnaireId)
                    ?? new Questionnaire();

            FormModel.Agreement = await _context.Agreements
                .Include(a => a.Customer)
                .Include(a => a.Answers)
                        .ThenInclude(da => da.DependentAnswers)
                .FirstOrDefaultAsync(a => a.AgreementID == agreementId);

            Agreement = FormModel.Agreement ?? new CustomerAgreements.Models.Agreement();

            if (FormModel.Agreement?.Customer != null)
            {
                FormModel.Customer = FormModel.Agreement.Customer;
            }
            else
            {
                FormModel.Customer = new Customer();
            }

            var instructions = await _context.Notifications
            .Where(n => n.NotificationKey == "Instructions")
            .FirstOrDefaultAsync();

            Instructions = instructions?.Text ?? "No instructions available.";

            var acknowledgement = await _context.Notifications
            .Where(n => n.NotificationKey == "Acknowledgement")
            .FirstOrDefaultAsync();

            Acknowledgement = acknowledgement?.Text ?? "No acknowledgement available.";
        }

        private async Task LoadQuestionnaireFromApiAsync(int questionnaireId, int agreementId)
        {
            var client = _httpClientFactory.CreateClient("CustomerAgreementsApi");

            var response = await client.GetAsync($"api/agreements/{agreementId}/edit-payload");
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<EditAgreementPayloadDto>();

            if (payload == null)
            {
                FormModel.Questionnaire = new Questionnaire();
                FormModel.Customer = new Customer();
                FormModel.Agreement = new CustomerAgreements.Models.Agreement();
                Instructions = "No instructions available.";
                Acknowledgement = "No acknowledgement available.";
                return;
            }

            if (payload.QuestionnaireId != questionnaireId)
            {
                throw new InvalidOperationException(
                    $"Payload questionnaireId {payload.QuestionnaireId} did not match route questionnaireId {questionnaireId}.");
            }

            Instructions = payload.Instructions;
            Acknowledgement = payload.Acknowledgement;

            FormModel.Customer = new Customer
            {
                CompanyName = payload.Customer.CompanyName,
                ContactName = payload.Customer.ContactName,
                EmailAddress = payload.Customer.EmailAddress
            };

            FormModel.Agreement = new CustomerAgreements.Models.Agreement
            {
                AgreementID = payload.AgreementId,
                QuestionnaireID = payload.QuestionnaireId,
                Status = payload.Status,
                CustomerName = payload.Customer.CompanyName,
                CustomerEmail = payload.Customer.EmailAddress
            };

            Agreement = FormModel.Agreement;

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

            ApplyAnswersToFormModel(payload);

            _logger.LogInformation(
                "Loaded agreement {AgreementId} via API.",
                agreementId);
        }

        private void ApplyAnswersToFormModel(EditAgreementPayloadDto payload)
        {
            // Safety
            if (payload == null)
                return;

            // Build quick lookup: QuestionID -> Question definition (from questionnaire shape)
            var questionById = FormModel.Questionnaire.Sections?
                .SelectMany(s => s.Questions ?? new List<Question>())
                .ToDictionary(q => q.QuestionID, q => q)
                ?? new Dictionary<int, Question>();

            var answers = new List<Answer>();

            foreach (var dto in payload.Answers ?? new List<AnswerDto>())
            {
                // Some payloads might still send checkbox selections as Values[]
                // Normalize so we always process "one selection/value" per loop.
                var normalized = new List<(string? value, int? questionListId, List<DependentAnswerDto>? deps)>();

                if (dto.Values != null && dto.Values.Count > 0)
                {
                    foreach (var v in dto.Values)
                        normalized.Add((v, dto.QuestionListId, dto.DependentAnswers));
                }
                else
                {
                    normalized.Add((dto.Value, dto.QuestionListId, dto.DependentAnswers));
                }

                foreach (var (value, incomingQuestionListId, deps) in normalized)
                {
                    // Get the question definition so we know AnswerType, lists, dependent types, etc.
                    questionById.TryGetValue(dto.QuestionId, out var questionDef);

                    // If API did not supply QuestionListId, try resolve it by matching ListValue
                    int? resolvedQuestionListId = incomingQuestionListId;

                    if (!resolvedQuestionListId.HasValue &&
                        questionDef?.QuestionLists != null &&
                        !string.IsNullOrWhiteSpace(value))
                    {
                        var match = questionDef.QuestionLists.FirstOrDefault(ql => ql.ListValue == value);
                        if (match != null)
                            resolvedQuestionListId = match.QuestionListID;
                    }

                    // Handle Date questions: store in DateAnswer, not Text
                    DateTime? parsedDate = null;
                    var isDateQuestion = string.Equals(questionDef?.AnswerType, "Date", StringComparison.OrdinalIgnoreCase);

                    if (isDateQuestion && !string.IsNullOrWhiteSpace(value))
                    {
                        // API sends yyyy-MM-dd, TryParse handles it fine
                        if (DateTime.TryParse(value, out var dt))
                            parsedDate = dt;
                    }

                    var answer = new Answer
                    {
                        AgreementID = payload.AgreementId,
                        QuestionnaireID = payload.QuestionnaireId,
                        QuestionID = dto.QuestionId,
                        QuestionListID = resolvedQuestionListId,

                        Text = isDateQuestion ? null : value,
                        DateAnswer = isDateQuestion ? parsedDate : null,

                        DependentAnswers = new List<DependentAnswer>()
                    };

                    // Add dependent answers (only if QuestionListID exists)
                    if (deps != null && deps.Count > 0 && resolvedQuestionListId.HasValue)
                    {
                        // Find the selected list item definition (so we can detect dependent date types)
                        var selectedListDef = questionDef?.QuestionLists?
                            .FirstOrDefault(ql => ql.QuestionListID == resolvedQuestionListId.Value);

                        foreach (var depDto in deps)
                        {
                            // Find dependent question definition for type info
                            var depDef = selectedListDef?.DependentQuestions?
                                .FirstOrDefault(dq => dq.DependentQuestionID == depDto.DependentQuestionId);

                            var depIsDate = string.Equals(depDef?.DependentAnswerType, "Date", StringComparison.OrdinalIgnoreCase);

                            DateTime? depParsedDate = null;
                            if (depIsDate && !string.IsNullOrWhiteSpace(depDto.Value))
                            {
                                if (DateTime.TryParse(depDto.Value, out var depDt))
                                    depParsedDate = depDt;
                            }

                            answer.DependentAnswers.Add(new DependentAnswer
                            {
                                AgreementID = payload.AgreementId,
                                QuestionnaireID = payload.QuestionnaireId,
                                QuestionID = dto.QuestionId,
                                QuestionListID = resolvedQuestionListId.Value,
                                DependentQuestionID = depDto.DependentQuestionId,

                                Answer = depIsDate ? null : depDto.Value,
                                DateAnswer = depIsDate ? depParsedDate : null
                            });
                        }
                    }

                    answers.Add(answer);
                }
            }

            // Attach answers to the FormModel agreement so your partials can prefill inputs
            FormModel.Agreement.Answers = answers;
        }



    }
}
