using CustomerAgreements.Data;
using CustomerAgreements.Models;
using CustomerAgreements.Pages.Questionnaires;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System.Diagnostics.Metrics;
using System.Linq;

namespace CustomerAgreements.Pages
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

        public List<CustomerAgreements.Models.Agreement> Agreements { get; set; } = new();
        public Questionnaire Questionnaire { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortField { get; set; } = "CompanyName";

        public SelectList CompanyNames { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedCompanyName { get; set; }

        public async Task OnGetAsync()
        {
            _logger.LogInformation($"User Viewed Core Site",
                            User.Identity?.Name ?? "Anonymous",
                            0,
                            DateTime.UtcNow);

            var query = _context.Agreements
                .Include(a => a.Customer)
                .Where(a => a.Status == "Submitted")
                .OrderByDescending(a => a.SubmittedDate)
                .AsQueryable();

            // Search filtering (by company name)
            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(a => a.Customer.CompanyName.Contains(SearchString));
            }

            // Dropdown filter
            if (!string.IsNullOrEmpty(SelectedCompanyName))
            {
                query = query.Where(a => a.Customer.CompanyName == SelectedCompanyName);
            }

            // Sorting
            switch (SortField)
            {
                case "ContactName":
                    query = query.OrderBy(a => a.Customer.ContactName);
                    break;

                case "CompanyName":
                    query = query.OrderBy(a => a.Customer.CompanyName);
                    break;

                case "EmailAddress":
                    query = query.OrderBy(a => a.Customer.EmailAddress);
                    break;

                default:
                    query = query.OrderByDescending(a => a.SubmittedDate);
                    break;
            }

            Agreements = await query.ToListAsync();

            // Build company name dropdown from agreements (not from all customers)
            CompanyNames = new SelectList(
                await _context.Agreements
                    .Where(a => a.Status == "Submitted")
                    .Select(a => a.Customer.CompanyName)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToListAsync()
            );

            Questionnaire = await _context.Questionnaires
                .Where(q => q.Status == "Active")
                .OrderBy(q => q.QuestionnaireID)
                .FirstOrDefaultAsync() ?? new Questionnaire();

            //if user is not admin, redirect to create agreement page
            //return RedirectToPage("/Agreements/Create");

        }
    }
}
