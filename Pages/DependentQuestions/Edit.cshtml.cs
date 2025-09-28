using CustomerAgreements.Data;
using CustomerAgreements.Helpers;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CustomerAgreements.Pages.DependentQuestions
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
        public Question DependentQuestion { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id, int questionnaireId)
        {
            DependentQuestion = await _context.DependentQuestions
            .Include(q => q.Section)
                .ThenInclude(s => s.Questionnaire)
            .Include(q => q.QuestionLists)
            .FirstOrDefaultAsync(q => q.ID == id
                                   && q.QuestionnaireID == questionnaireId);


            if (DependentQuestion == null)
            {
                return NotFound();
            }
            
            ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions();
                return Page();
            }

            try
            {
                var existingQuestion = await _context.Questions
                .Include(q => q.Section)
                .FirstOrDefaultAsync(q => q.ID == id);

                if (existingQuestion == null)
                {
                    return NotFound();
                }

                var originalAnswerType = (existingQuestion.AnswerType ?? "").ToLower();

                existingQuestion.QuestionTitle = Question.QuestionTitle;
                existingQuestion.QuestionText = Question.QuestionText;
                existingQuestion.AnswerType = Question.AnswerType;
                existingQuestion.IsRequired = Question.IsRequired;
                existingQuestion.SortOrder = Question.SortOrder;
                existingQuestion.Text = existingQuestion.QuestionText;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} created new question",
                            User.Identity?.Name ?? "Anonymous",
                            Question.QuestionID,
                            DateTime.UtcNow);

                var newAnswerType = (Question.AnswerType ?? "").ToLower();

                if (newAnswerType.Contains("list") && !originalAnswerType.Contains("list"))
                {
                    return RedirectToPage("/Questions/Edit", new
                    {
                        id = id,
                        questionnaireId = Question.QuestionnaireID
                    });
                }
                else
                {
                    return RedirectToPage("/Questionnaires/Edit", new { id = existingQuestion.QuestionnaireID });
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving question by user {User}: {Message}",
                    User.Identity?.Name ?? "Anonymous",
                    ex.Message);

                return Page();
            }            
        }

        public async Task<IActionResult> OnPostDeleteListItemAsync(int questionListId, int questionId, int questionnaireId, int questionUniqueId)
        {
            try
            {
                var questionListItem = await _context.QuestionLists.FindAsync(questionListId);

                if (questionListItem != null)
                {
                    _context.QuestionLists.Remove(questionListItem);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"User {User} deleted list item {questionListId}",
                User.Identity?.Name ?? "Anonymous",
                questionListId,
                DateTime.UtcNow);

                return RedirectToPage(new { id = questionUniqueId, questionnaireId = questionnaireId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list item by user {User}: {Message}",
                    User.Identity?.Name ?? "Anonymous",
                    ex.Message);

                return Page();
            }
        }
    }
}
