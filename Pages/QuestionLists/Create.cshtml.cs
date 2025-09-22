using CustomerAgreements.Data;
using CustomerAgreements.Helpers;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace CustomerAgreements.Pages.QuestionLists
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
        public QuestionList QuestionList { get; set; } = new QuestionList();

        public IActionResult OnGet(int questionId, int questionnaireId)
        {
            QuestionList = new QuestionList
            {
                QuestionID = questionId,
                QuestionnaireID = questionnaireId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _context.QuestionLists.Add(QuestionList);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} added new list item",
                            User.Identity?.Name ?? "Anonymous",
                            QuestionList.QuestionListID,
                            DateTime.UtcNow);

            return RedirectToPage("/Question/Edit", new { id = QuestionList.QuestionID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding list item. {ex.Message}",
                    QuestionList.ListValue,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to save list item. See logs for details.");
                return Page();
            }
        }
    }
}
