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

namespace CustomerAgreements.Pages.Questions
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
        public Question Question { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id, int questionnaireId)
        {         
            Question = await _context.Questions
            .Include(q => q.Section)
                .ThenInclude(s => s.Questionnaire)
            .Include(q => q.QuestionLists)
            .FirstOrDefaultAsync(q => q.ID == id
                                   && q.QuestionnaireID == questionnaireId);


            if (Question == null)
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

                existingQuestion.QuestionTitle = Question.QuestionTitle;
                existingQuestion.QuestionText = Question.QuestionText;
                existingQuestion.AnswerType = Question.AnswerType;
                existingQuestion.IsRequired = Question.IsRequired;
                existingQuestion.SortOrder = Question.SortOrder;
                existingQuestion.Text = existingQuestion.QuestionText;
                await _context.SaveChangesAsync();

                return RedirectToPage("/Questionnaires/Edit", new { id = existingQuestion.  QuestionnaireID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving question by user {User}: {Message}",
                    User.Identity?.Name ?? "Anonymous",
                    ex.Message);

                return Page();
            }            
        }
    }
}
