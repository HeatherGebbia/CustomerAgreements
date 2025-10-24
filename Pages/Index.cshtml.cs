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
        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext _context;

        public IndexModel(ILogger<CreateModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public List<Customer> Customers { get; set; }        

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortField { get; set; } = "CompanyName";

        public SelectList CompanyNames { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedCompanyName { get; set; }

        [BindProperty]
        public Questionnaire Questionnaire { get; set; } = default!;

        public async Task OnGetAsync()
        {
            _logger.LogInformation($"User Viewed Core Site",
                            User.Identity?.Name ?? "Anonymous",
                            0,
                            DateTime.UtcNow);

            Questionnaire = await _context.Questionnaires
                .FirstOrDefaultAsync(q => q.Status == "Active");

            var customers = from c in _context.Customers
                            select c;

            if (!string.IsNullOrEmpty(SearchString))
            {
                customers = customers.Where(c => c.CompanyName.Contains(SearchString));
            }

            switch (SortField)
            {
                case "ContactName":
                    customers = customers.OrderBy(c => c.ContactName);
                    break;
                case "CompanyName":
                    customers = customers.OrderBy(c => c.CompanyName);
                    break;
                case "EmailAddress":
                    customers = customers.OrderBy(c => c.EmailAddress);
                    break;
            }

            IQueryable<string> customerQuery = from c in _context.Customers
                                                orderby c.CompanyName
                                                select c.CompanyName;

            CompanyNames = new SelectList(await customerQuery.Distinct().ToListAsync());

            if (!string.IsNullOrEmpty(SelectedCompanyName))
            {
                customers = customers.Where(c => c.CompanyName == SelectedCompanyName);
            }


            Customers = await customers.ToListAsync();
        }

        public IActionResult OnPost()
        {
            //if user is not admin, redirect to create agreement page
            return RedirectToPage("/Agreements/Create");
        }
    }
}
