using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CustomerAgreements.Data;
using CustomerAgreements.Models;
using CustomerAgreements.Services;

namespace CustomerAgreements.Pages.Agreements
{
    public class EditModel : PageModel
    {
        private readonly ILogger<EditModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly AgreementResponseService _agreementService;

        public EditModel(ILogger<EditModel> logger, ApplicationDbContext context, AgreementResponseService agreementService)
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

        private async Task LoadQuestionnaireAsync(int questionnaireId, int agreementId)
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
    }
}
