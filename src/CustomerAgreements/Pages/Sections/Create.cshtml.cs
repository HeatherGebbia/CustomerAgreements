using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomerAgreements.Pages.Sections
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
        public Section Section { get; set; } = new Section();

        public IActionResult OnGet(int questionnaireId)
        {
            Section = new Section
            {
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
                _context.Sections.Add(Section);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} created new section",
                        User.Identity?.Name ?? "Anonymous",
                        Section.SectionID,
                        DateTime.UtcNow);

                return RedirectToPage("/Questionnaires/Edit", new { id = Section.QuestionnaireID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating section. {ex.Message}",
                    Section.Text,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to save section. See logs for details.");
                return Page();
            }
        }

    }
}
