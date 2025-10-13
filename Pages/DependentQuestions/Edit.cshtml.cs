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
        public DependentQuestion DependentQuestion { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int dependentQuestionId)
        {
            _logger.LogInformation($"User Viewed Dependent Questions edit page",
                            User.Identity?.Name ?? "Anonymous",
                            0,
                            DateTime.UtcNow);

            DependentQuestion = await _context.DependentQuestions
                .Include(dq => dq.DependentQuestionLists)
            .FirstOrDefaultAsync(q => q.DependentQuestionID == dependentQuestionId);

            if (DependentQuestion == null)
            {
                return NotFound();
            }
            
            ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(int dependentQuestionId)
        {
            if (!ModelState.IsValid)
            {
                ViewData["AnswerTypeOptions"] = AnswerTypeHelper.GetAnswerTypeOptions();
                return Page();
            }

            try
            {
                var existingDependentQuestion = await _context.DependentQuestions
                .FirstOrDefaultAsync(q => q.DependentQuestionID == dependentQuestionId);

                if (existingDependentQuestion == null)
                {
                    return NotFound();
                }

                var originalDependentAnswerType = (existingDependentQuestion.DependentAnswerType ?? "").ToLower();

                existingDependentQuestion.DependentQuestionTitle = DependentQuestion.DependentQuestionTitle;
                existingDependentQuestion.DependentQuestionText = DependentQuestion.DependentQuestionText;
                existingDependentQuestion.DependentAnswerType = DependentQuestion.DependentAnswerType;
                existingDependentQuestion.IsRequired = DependentQuestion.IsRequired;
                existingDependentQuestion.SortOrder = DependentQuestion.SortOrder;
                existingDependentQuestion.Text = existingDependentQuestion.DependentQuestionText;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} edited dependent question",
                            User.Identity?.Name ?? "Anonymous",
                            DependentQuestion.DependentQuestionID,
                            DateTime.UtcNow);

                var newDependentAnswerType = (DependentQuestion.DependentAnswerType ?? "").ToLower();

                if (newDependentAnswerType.Contains("list") && !originalDependentAnswerType.Contains("list"))
                {
                    return RedirectToPage("/DependentQuestions/Edit", new
                    {
                        dependentQuestionId = dependentQuestionId
                    });
                }
                else
                {
                    return RedirectToPage("/QuestionLists/Edit", new { questionListId = existingDependentQuestion.QuestionListID });
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving dependent question by user {User}: {Message}",
                    User.Identity?.Name ?? "Anonymous",
                    ex.Message);

                return Page();
            }            
        }

        public async Task<IActionResult> OnPostDeleteListItemAsync(int dependentQuestionListId, int dependentQuestionId)
        {
            try
            {
                var dependentQuestionListItem = await _context.DependentQuestionLists.FindAsync(dependentQuestionListId);

                if (dependentQuestionListItem != null)
                {
                    _context.DependentQuestionLists.Remove(dependentQuestionListItem);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"User {User} deleted dependent list item {dependentQuestionListId}",
                User.Identity?.Name ?? "Anonymous",
                dependentQuestionListId,
                DateTime.UtcNow);

                return RedirectToPage(new { dependentQuestionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dependent list item by user {User}: {Message}",
                    User.Identity?.Name ?? "Anonymous",
                    ex.Message);

                return Page();
            }
        }
    }
}
