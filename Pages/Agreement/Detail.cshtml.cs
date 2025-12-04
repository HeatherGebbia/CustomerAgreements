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
                FormModel.Questionnaire = await _context.Questionnaires
                    .Include(q => q.Sections)
                        .ThenInclude(s => s.Questions)
                        .ThenInclude(ql => ql.QuestionLists)
                        .ThenInclude(dq => dq.DependentQuestions)
                    .FirstOrDefaultAsync(q => q.QuestionnaireID == questionnaireId)
                    ?? new Questionnaire();

                FormModel.Agreement = await _context.Agreements
                    .Include(a => a.Customer)
                    .Include(a => a.Answers)
                        .ThenInclude(da => da.DependentAnswers)
                    .FirstOrDefaultAsync(a => a.AgreementID == agreementId);

                if (FormModel.Agreement?.Customer != null)
                    FormModel.Customer = FormModel.Agreement.Customer;

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

    }
}
