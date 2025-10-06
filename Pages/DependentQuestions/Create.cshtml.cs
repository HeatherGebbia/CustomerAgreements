using CustomerAgreements.Data;
using CustomerAgreements.Helpers;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace CustomerAgreements.Pages.DependentQuestions
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
        public DependentQuestion DependentQuestion { get; set; } = new DependentQuestion();

        public IActionResult OnGet(int sectionId, int questionnaireId, int questionId, int questionListId)
        {
            DependentQuestion = new DependentQuestion
            {
                SectionID = sectionId,
                QuestionnaireID = questionnaireId,
                QuestionID = questionId,
                QuestionListID = questionListId,
            };

            ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions();
            return Page();
        }        

        public async Task<IActionResult> OnPostAsync(int questionId)
        {
            if (!ModelState.IsValid)
            {
                ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions();
                return Page();
            }

            try
            {    
                _context.DependentQuestions.Add(DependentQuestion);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} created new dependent question",
                            User.Identity?.Name ?? "Anonymous",
                            DependentQuestion.DependentQuestionID,
                            DateTime.UtcNow);

                var newDependentQuestionId = DependentQuestion.DependentQuestionID;
                var answerType = (DependentQuestion.DependentAnswerType ?? "").ToLower();

                if (answerType.Contains("list"))
                {
                    return RedirectToPage("/DependentQuestions/Edit", new
                    {
                        id = newDependentQuestionId,
                        questionnaireId = DependentQuestion.QuestionnaireID,
                        questionId = DependentQuestion.QuestionID
                    });
                }                

                return RedirectToPage("/Question/Edit", new { id = questionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating dependent question. {ex.Message}",
                    DependentQuestion.DependentQuestionTitle,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to save dependent question. See logs for details.");
                return Page();
            }
        }
    }
}
