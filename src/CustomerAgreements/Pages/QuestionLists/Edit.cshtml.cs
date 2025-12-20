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

namespace CustomerAgreements.Pages.QuestionLists
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
        public QuestionList QuestionList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int questionListId)
        {
            _logger.LogInformation($"User Viewed Question Lists edit page",
                            User.Identity?.Name ?? "Anonymous",
                            0,
                            DateTime.UtcNow);

            QuestionList = await _context.QuestionLists
                .Include(q => q.DependentQuestions)
                .FirstOrDefaultAsync(ql => ql.QuestionListID == questionListId);

            if (QuestionList == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int questionId, int questionnaireId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }            

            try
            {
                _context.Attach(QuestionList).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {User} updated list item {Id}",
                    User.Identity?.Name ?? "Anonymous",
                    QuestionList.QuestionListID);

                return RedirectToPage("/Questions/Edit", new
                {
                    questionId,
                    questionnaireId
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving list item by user {User}: {Message}",
                   User.Identity?.Name ?? "Anonymous",
                   ex.Message);

                return Page();
            }

            //return RedirectToPage("/Questions/Edit", new { id = QuestionList.QuestionID, questionnaireId = QuestionList.QuestionnaireID });
        }

        public async Task<IActionResult> OnPostDeleteDependentQuestionAsync(int dependentQuestionId, int questionListId)
        {
            try
            {
                var dependentQuestion = await _context.DependentQuestions
                    .Include(q => q.DependentQuestionLists)
                    .FirstOrDefaultAsync(q => q.DependentQuestionID == dependentQuestionId);

                if (dependentQuestion != null)
                {
                    // Delete child DependentQuestionLists first
                    if (dependentQuestion.DependentQuestionLists != null && dependentQuestion.DependentQuestionLists.Any())
                    {
                        _context.DependentQuestionLists.RemoveRange(dependentQuestion.DependentQuestionLists);
                    }

                    // Delete parent Dependent Question
                    _context.DependentQuestions.Remove(dependentQuestion);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"User {User} deleted dependent question {dependentQuestionId}",
                User.Identity?.Name ?? "Anonymous",
                dependentQuestionId,
                DateTime.UtcNow);

                return RedirectToPage(new { questionListId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dependent question by user {User}: {Message}",
                    User.Identity?.Name ?? "Anonymous",
                    ex.Message);

                return Page();
            }
        }
    }
}
