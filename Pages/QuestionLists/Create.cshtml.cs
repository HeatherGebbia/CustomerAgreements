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

        public IActionResult OnGet(int questionUniqueId, int questionId, int questionnaireId, int sectionId)
        {
            QuestionList = new QuestionList
            {
                QuestionID = questionId,           
                QuestionnaireID = questionnaireId,
                SectionID = sectionId
            };

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
                _context.QuestionLists.Add(QuestionList);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} added new list item",
                            User.Identity?.Name ?? "Anonymous",
                            QuestionList.QuestionListID,
                            DateTime.UtcNow);

                return RedirectToPage("/Questions/Edit", new
                {
                    id = questionUniqueId,
                    questionnaireId = QuestionList.QuestionnaireID
                });
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
