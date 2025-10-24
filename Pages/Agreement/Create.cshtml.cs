using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace CustomerAgreements.Pages.Agreements
{
    public class CreateModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext _context;

        public CreateModel(ILogger<CreateModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
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


        public async Task<IActionResult> OnGetAsync(int questionnaireId)
        {
            try
            {
                LoadQuestionnaireAsync(questionnaireId).Wait();

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
            // Determine which button was clicked
            var isSubmit = ActionType == "Submit";

            await LoadQuestionnaireAsync(questionnaireId);

            if (!isSubmit)
            {
                // Remove validation errors for questionnaire questions 
                var keysToRemove = ModelState.Keys
                    .Where(k => k.Contains("Questionnaire.Sections")
                             || k.Contains("Questions")
                             || k.Contains("QuestionLists")
                             || k.Contains("Answer")
                             || k.Contains("DependentAnswer"))
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
                _context.Customers.Add(FormModel.Customer);
                await _context.SaveChangesAsync();

                FormModel.Agreement.CustomerID = FormModel.Customer.CustomerID;
                FormModel.Agreement.QuestionnaireID = questionnaireId;
                FormModel.Agreement.CustomerName = FormModel.Customer.CompanyName;
                FormModel.Agreement.CustomerEmail= FormModel.Customer.EmailAddress;

                if (isSubmit)
                {
                    FormModel.Agreement.SubmittedDate = DateTime.UtcNow;
                    FormModel.Agreement.Status = "Submitted";
                }
                else
                {
                    FormModel.Agreement.Status = "Draft";
                }

                _context.Agreements.Add(FormModel.Agreement);
                await _context.SaveChangesAsync();

                if (FormModel.Questionnaire?.Sections != null)
                {
                    foreach (var section in FormModel.Questionnaire.Sections)
                    {
                        if (section.Questions == null) continue;

                        foreach (var question in section.Questions)
                        {
                            string fieldName = $"question_{question.QuestionID}";
                            string? userInput = Request.Form[fieldName];

                            // Skip unanswered non-required questions
                            if (string.IsNullOrWhiteSpace(userInput) && !question.IsRequired)
                                continue;

                            var answer = new Answer
                            {
                                AgreementID = FormModel.Agreement.AgreementID,
                                QuestionID = question.QuestionID,
                                QuestionnaireID = questionnaireId,
                                SectionID = question.SectionID
                            };

                            // Save based on answer type
                            if (question.AnswerType == "Date")
                            {
                                if (DateTime.TryParse(userInput, out var parsedDate))
                                {
                                    answer.DateAnswer = parsedDate;
                                }
                            }
                            else
                            {
                                answer.Text = userInput;
                            }

                                _context.Answers.Add(answer);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                    
                if (isSubmit)
                {
                    return RedirectToPage("/Agreement/Detail", new { questionnaireId, agreementId = FormModel.Agreement.AgreementID });
                }
                else
                {
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

        private async Task LoadQuestionnaireAsync(int questionnaireId)
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
        }
    }
}
