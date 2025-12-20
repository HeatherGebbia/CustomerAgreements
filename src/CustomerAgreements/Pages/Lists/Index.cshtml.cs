using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace CustomerAgreements.Pages.Lists
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

        public IList<List> Lists { get; set; } = new List<List>();

        public async Task OnGetAsync()
        {
            _logger.LogInformation($"User Viewed Lists page",
                            User.Identity?.Name ?? "Anonymous",
                            0,
                            DateTime.UtcNow);

            // Load all list rows into memory
            var allLists = await _context.Lists
                .AsNoTracking()
                .ToListAsync();

            // Group in-memory by ListName and take one row per group
            Lists = allLists
                .GroupBy(l => l.ListName)
                .Select(g => g.First())
                .OrderBy(l => l.ListName)
                .ToList();

        }


    }
}
