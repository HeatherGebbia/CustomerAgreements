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

        public async Task<IActionResult> OnGetAsync(int questionListId, int questionUniqueId, int questionnaireId)
        {
            QuestionList = await _context.QuestionLists
                .Include(q => q.DependentQuestions)
                .FirstOrDefaultAsync(ql => ql.QuestionListID == questionListId
                                        && ql.QuestionnaireID == questionnaireId);

            if (QuestionList == null)
            {
                return NotFound();
            }

            ViewData["QuestionUniqueId"] = questionUniqueId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int questionUniqueId)
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
                    id = questionUniqueId,
                    questionnaireId = QuestionList.QuestionnaireID
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

        public async Task<IActionResult> OnPostDeleteDependentQuestionAsync(int dependentQuestionId, int questionListId, int questionUniqueId, int questionnaireId)
        {
            try
            {
                var dependentQuestion = await _context.DependentQuestions.FindAsync(dependentQuestionId);

                if (dependentQuestion != null)
                {
                    _context.DependentQuestions.Remove(dependentQuestion);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"User {User} deleted dependent question {dependentQuestionId}",
                User.Identity?.Name ?? "Anonymous",
                dependentQuestionId,
                DateTime.UtcNow);

                return RedirectToPage(new { questionListId = questionListId, questionUniqueId = questionUniqueId, questionnaireId = questionnaireId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dependent question by user {User}: {Message}",
                    User.Identity?.Name ?? "Anonymous",
                    ex.Message);

                return Page();
            }
        }

        //private bool QuestionListExists(int id)
        //{
        //    return _context.QuestionLists.Any(e => e.QuestionListID == id);
        //}
    }
}
