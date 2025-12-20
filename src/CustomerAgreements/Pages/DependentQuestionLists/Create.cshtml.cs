using CustomerAgreements.Data;
using CustomerAgreements.Helpers;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace CustomerAgreements.Pages.DependentQuestionLists
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
        public DependentQuestionList DependentQuestionList { get; set; } = new DependentQuestionList();

        public IActionResult OnGet(int questionId, int dependentQuestionId, int questionnaireId, int sectionId)
        {
            DependentQuestionList = new DependentQuestionList
            {
                DependentQuestionID = dependentQuestionId,
                QuestionID = questionId,           
                QuestionnaireID = questionnaireId,
                SectionID = sectionId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int dependentQuestionId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _context.DependentQuestionLists.Add(DependentQuestionList);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} added new dependent list item",
                            User.Identity?.Name ?? "Anonymous",
                            DependentQuestionList.DependentQuestionListID,
                            DateTime.UtcNow);

                return RedirectToPage("/DependentQuestions/Edit", new
                {
                    dependentQuestionId = dependentQuestionId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding dependent list item. {ex.Message}",
                    DependentQuestionList.ListValue,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to save dependent list item. See logs for details.");
                return Page();
            }
        }
    }
}
