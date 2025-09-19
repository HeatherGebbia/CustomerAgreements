using CustomerAgreements.Data;
using CustomerAgreements.Helpers;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace CustomerAgreements.Pages.Questions
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
        public Question Question { get; set; } = new Question();

        public IActionResult OnGet(int sectionId, int questionnaireId)
        {
            Question = new Question
            {
                SectionID = sectionId,
                QuestionnaireID = questionnaireId
            };

            ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions();
            return Page();
        }        

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions();
                return Page();
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(Question.QuestionTitle))
                {
                    Question.QuestionKey = Question.QuestionTitle.Replace(" ", "");
                }

                Question.Text = Question.QuestionText;

                _context.Questions.Add(Question);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {User} created new question",
                        User.Identity?.Name ?? "Anonymous",
                        Question.QuestionID,
                        DateTime.UtcNow);

            return RedirectToPage("/Questionnaires/Edit", new { id = Question.QuestionnaireID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating question. {ex.Message}",
                    Question.QuestionTitle,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to save question. See logs for details.");
                return Page();
            }
        }
    }
}
