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

        public IActionResult OnGet(int questionId, int questionnaireId, int sectionId)
        {
            QuestionList = new QuestionList
            {
                QuestionID = questionId,
                QuestionnaireID = questionnaireId,
                SectionID = sectionId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int questionId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var ListItem = new QuestionList
                {
                    QuestionID = QuestionList.QuestionID,
                    QuestionnaireID = QuestionList.QuestionnaireID,
                    SectionID = QuestionList.SectionID,
                    ListValue = QuestionList.ListValue,
                    Conditional = QuestionList.Conditional,
                    SortOrder = QuestionList.SortOrder,
                    SendEmail = QuestionList.SendEmail,
                    NotificationID = QuestionList.NotificationID
                };

                _context.QuestionLists.Add(ListItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} added new list item",
                            User.Identity?.Name ?? "Anonymous",
                            QuestionList.QuestionListID,
                            DateTime.UtcNow);

            return RedirectToPage("/Questions/Edit", new { id = questionId, questionnaireId = QuestionList.QuestionnaireID });
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
