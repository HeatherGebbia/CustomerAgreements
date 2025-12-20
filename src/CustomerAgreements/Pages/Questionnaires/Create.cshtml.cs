using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomerAgreements.Pages.Questionnaires
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
        public Questionnaire Questionnaire { get; set; } = new Questionnaire();

        public void OnGet()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading new questionnaire by user {User}. {ex.Message}",
                    -1,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);
            }            
        }

        public async Task<IActionResult> OnPostSave()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _context.Questionnaires.Add(Questionnaire);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} created new questionnaire",
                    User.Identity?.Name ?? "Anonymous",
                    Questionnaire.QuestionnaireID,
                    DateTime.UtcNow);

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating questionnaire. {ex.Message}",
                    Questionnaire.QuestionnaireName,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to save questionnaire. See logs for details.");
                return Page();
            }
        }
    }
}
