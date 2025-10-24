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

namespace CustomerAgreements.Pages.Agreements
{
    public class EditModel : PageModel
    {
        private readonly ILogger<EditModel> _logger;
        private readonly ApplicationDbContext _context;

        public EditModel(ILogger<EditModel> logger, ApplicationDbContext context)
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

        [BindProperty]
        public string ActionType { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int questionnaireId, int agreementId)
        {
            try
            {
                LoadQuestionnaireAsync(questionnaireId, agreementId).Wait();
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
            // Determine which button was clicked
            var isSubmit = ActionType == "Submit";

            if (!isSubmit)
            {
                // Remove validation errors for questionnaire questions
                var keysToRemove = ModelState.Keys
                    .Where(k => k.Contains("Questionnaire.Sections") || k.Contains("Questions") || k.Contains("QuestionLists"))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    ModelState.Remove(key);
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadQuestionnaireAsync(questionnaireId, agreementId);
                return Page();
            }

            try
            {                
                if (isSubmit)
                {
                    FormModel.Agreement.SubmittedDate = DateTime.UtcNow;
                    FormModel.Agreement.Status = "Submitted";
                }

                _context.Agreements.Add(FormModel.Agreement);
                await _context.SaveChangesAsync();

                var agreement = await _context.Agreements
                    .Include(a => a.Answers)
                        .ThenInclude(da => da.DependentAnswers)
                    .FirstOrDefaultAsync(a => a.AgreementID == FormModel.Agreement.AgreementID);

                if (agreement != null)
                {
                    foreach (var answer in agreement.Answers)
                    {
                        if (answer.DependentAnswers.Any())
                        {
                            _context.DependentAnswers.RemoveRange(answer.DependentAnswers);
                        }
                    }

                    _context.Answers.RemoveRange(agreement.Answers);
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
