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

        [BindProperty]
        public QuestionLibrary QuestionLibrary { get; set; } = new QuestionLibrary();

        public IActionResult OnGet(int sectionId, int questionnaireId)
        {
            Question = new Question
            {
                SectionID = sectionId,
                QuestionnaireID = questionnaireId,
                QuestionLists = new List<QuestionList>()
            };

            ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions(false);
            return Page();
        }        

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions(false);
                return Page();
            }

            try
            {
                var library = new QuestionLibrary
                {
                    QuestionTitle = Question.QuestionTitle,
                    QuestionText = Question.QuestionText,
                    Text = Question.QuestionText,
                    QuestionKey = Question.QuestionTitle.Replace(" ", ""),
                    AnswerType = Question.AnswerType,
                    IsRequired = Question.IsRequired,
                    SortOrder = Question.SortOrder
                };

                _context.QuestionLibrary.Add(library);
                await _context.SaveChangesAsync();

                Question.QuestionID = library.QuestionID;
                Question.QuestionKey = library.QuestionKey;
                Question.Text = library.Text;
                

                _context.Questions.Add(Question);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} created new question",
                            User.Identity?.Name ?? "Anonymous",
                            Question.QuestionID,
                            DateTime.UtcNow);

                var newQuestionId = Question.QuestionID;
                var answerType = (Question.AnswerType ?? "").ToLower();

                if (answerType.Contains("list"))
                {
                    return RedirectToPage("/Questions/Edit", new
                    {
                        questionId = newQuestionId,
                        questionnaireId = Question.QuestionnaireID
                    });
                }                

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
