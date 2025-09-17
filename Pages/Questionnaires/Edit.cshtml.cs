using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using static System.Collections.Specialized.BitVector32;

namespace CustomerAgreements.Pages.Questionnaires
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
        public Questionnaire Questionnaire { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Questionnaire = await _context.Questionnaires
                    .Where(m => m.QuestionnaireID == id)
                    .Select(q => new Questionnaire
                    {
                        QuestionnaireID = q.QuestionnaireID,
                        QuestionnaireName = q.QuestionnaireName,
                        Status = q.Status,
                        Sections = q.Sections
                            .OrderBy(s => s.SortOrder)
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (Questionnaire == null)
                {
                    return NotFound();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading questionnaire {id} for editing by user {User}. {ex.Message}",
                    id,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);
                return Page();
            }

        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _context.Attach(Questionnaire).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} updated questionnaire.",
                    User.Identity?.Name ?? "Anonymous",
                    Questionnaire.QuestionnaireID,
                    DateTime.UtcNow);


                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating questionnaire by user {User}. {ex.Message}",
                    0,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteSectionAsync(int sectionId, int id)
        {

            try
            {
                var section = await _context.Sections.FindAsync(sectionId);

                if (section != null)
                {
                    _context.Sections.Remove(section);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"User {User} deleted section {sectionId}",
                User.Identity?.Name ?? "Anonymous",
                sectionId,
                DateTime.UtcNow);

                return RedirectToPage(new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting section by user {User}. {ex.Message}",
                    0,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);
                return Page();
            }
        }
    }
}
