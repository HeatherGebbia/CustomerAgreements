using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomerAgreements.Pages.Lists
{
    public class CreateModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext _context;

        public CreateModel(ILogger<CreateModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public List List { get; set; } = new ();

        public void OnGet()
        {
                   
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _context.Lists.Add(List);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} created new List",
                    User.Identity?.Name ?? "Anonymous",
                    List.ListID,
                    DateTime.UtcNow);

                return RedirectToPage("/Lists/Edit", new
                {
                    id = List.ListID
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating List. {ex.Message}",
                    List.ListName,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to save List. See logs for details.");
                return Page();
            }
        }
    }
}
