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
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IList<Questionnaire> Questionnaires { get; set; } = new List<Questionnaire>();

        public async Task OnGetAsync()
        {
            _logger.LogInformation($"User Viewed Questionnaires page",
                            User.Identity?.Name ?? "Anonymous",
                            0,
                            DateTime.UtcNow);

            Questionnaires = await _context.Questionnaires.ToListAsync();
        }

        
    }
}
