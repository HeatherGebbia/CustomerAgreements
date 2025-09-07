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
        //public SelectList Statuses { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                //Statuses = new SelectList(Statuses.Items.Cast<List>().Where(l => l.ListName == "Status"), nameof(List.ListID), nameof(List.ListValue));

                Questionnaire = await _context.Questionnaires.FindAsync(id);

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

        public async Task<IActionResult> OnPostAsync()
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
    }
}
