using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace CustomerAgreements.Pages.Questionnaires
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext _context;

        public IndexModel(ILogger<CreateModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IList<Questionnaire> Questionnaires { get; set; } = new List<Questionnaire>();

        public async Task OnGetAsync()
        {
            Questionnaires = await _context.Questionnaires.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            try
            {
                if (id.HasValue == false)
                {
                    return NotFound();
                }
                else
                {
                    //sample code from training to find and delete
                    Questionnaire Questionnaire = await _context.Questionnaires.FindAsync(id);

                    if (Questionnaire != null)
                    {
                        _context.Questionnaires.Remove(Questionnaire);
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"User {User} deleted questionnaire {id}",
                    User.Identity?.Name ?? "Anonymous",
                    id,
                    DateTime.UtcNow);

                    return RedirectToPage("./Index");
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting questionnaire {id} by user {User}.  {ex.Message}",
                    id,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);
                return Page();
            }

        }
    }
}
