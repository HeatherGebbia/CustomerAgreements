using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomerAgreements.Pages.Agreements
{
    public class DetailModel : PageModel
    {
        private readonly ILogger<DetailModel> _logger;
        private readonly ApplicationDbContext _context;

        public DetailModel(ILogger<DetailModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public CustomerAgreements.Models.Agreement Agreement { get; set; } = new CustomerAgreements.Models.Agreement();
        [BindProperty]
        public AgreementFormViewModel FormModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int questionnaireId, int agreementId)
        {
            try
            {
                // Load the questionnaire with sections and questions
                FormModel.Questionnaire = await _context.Questionnaires
                    .Include(q => q.Sections)
                        .ThenInclude(s => s.Questions)
                    .FirstOrDefaultAsync(q => q.QuestionnaireID == questionnaireId)
                    ?? new Questionnaire();

                // Initialize a new Agreement record
                FormModel.Agreement = new CustomerAgreements.Models.Agreement
                {
                    QuestionnaireID = questionnaireId
                };

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading agreement page. {ex.Message}",
                    Agreement.AgreementID,
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
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error viewing Agreement. {ex.Message}",
                    Agreement.AgreementID,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to view Agreement. See logs for details.");
                return Page();
            }
        }

    }
}
