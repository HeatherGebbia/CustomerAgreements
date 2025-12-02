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
using CustomerAgreements.Services;

namespace CustomerAgreements.Pages.Agreements
{
    public class CreateModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly AgreementResponseService _agreementService;

        public CreateModel(ILogger<CreateModel> logger, ApplicationDbContext context, AgreementResponseService agreementService)
        {
            _logger = logger;
            _context = context;
            _agreementService = agreementService;
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
            // Determine which button was clicked
            var isSubmit = ActionType == "Submit";

            await LoadQuestionnaireAsync(questionnaireId);

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
                _context.Customers.Add(FormModel.Customer);
                await _context.SaveChangesAsync();

                FormModel.Agreement.CustomerID = FormModel.Customer.CustomerID;
                FormModel.Agreement.QuestionnaireID = questionnaireId;
                FormModel.Agreement.CustomerName = FormModel.Customer.CompanyName;
                FormModel.Agreement.CustomerEmail= FormModel.Customer.EmailAddress;
                FormModel.Agreement.Status = isSubmit ? "Submitted" : "Draft";

                if (isSubmit)
                {
                    FormModel.Agreement.SubmittedDate = DateTime.UtcNow;
                }

                _context.Agreements.Add(FormModel.Agreement);
                await _context.SaveChangesAsync();

                await _agreementService.SaveOrUpdateAnswersFromFormAsync(questionnaireId, FormModel.Agreement, Request.Form, FormModel.Questionnaire);
                    
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
