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
                    .Include(q => q.Sections)
                        .ThenInclude(s => s.Questions)
                    .FirstOrDefaultAsync(q => q.QuestionnaireID == id);

                if (Questionnaire == null)
                {
                    _logger.LogWarning("Questionnaire {Id} not found for edit", id);
                    return NotFound();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading questionnaire {Id} for edit", id);
                return StatusCode(500, "An error occurred while loading the questionnaire.");
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

        public async Task<IActionResult> OnPostDeleteSectionAsync(int sectionId, int questionnaireId)
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

                return RedirectToPage(new { questionnaireId });
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

        public async Task<IActionResult> OnPostDeleteQuestionAsync(int questionId, int questionnaireId)
        {
            try
            {
                var question = await _context.Questions
                    .Include(q => q.Section)
                    .ThenInclude(q => q.Questionnaire)
                    .FirstOrDefaultAsync(q => q.ID == questionId
                               && q.QuestionnaireID == questionnaireId);

                if (question == null)
                {
                    return NotFound();
                }

                //var questionnaireId = question.Section.QuestionnaireID;

                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {User} deleted question {QuestionId}",
                    User.Identity?.Name ?? "Anonymous",
                    questionId);

                return RedirectToPage(new { id = questionnaireId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question by user {User}: {Message}",
                    User.Identity?.Name ?? "Anonymous",
                    ex.Message);

                return Page();
            }
        }


    }
}
