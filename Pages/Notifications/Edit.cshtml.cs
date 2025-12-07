using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using static System.Collections.Specialized.BitVector32;

namespace CustomerAgreements.Pages.Notifications
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
        public Notification Notification { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation($"User Viewed Notifications edit page",
                            User.Identity?.Name ?? "Anonymous",
                            0,
                            DateTime.UtcNow);

                Notification = await _context.Notifications
                    .FirstOrDefaultAsync(q => q.NotificationID == id);

                if (Notification == null)
                {
                    _logger.LogWarning("Notification {Id} not found for edit", id);
                    return NotFound();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Notification {Id} for edit", id);
                return StatusCode(500, "An error occurred while loading the Notification.");
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
                Notification.LastUpdatedDate = DateTime.UtcNow;
                _context.Attach(Notification).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} updated Notification.",
                    User.Identity?.Name ?? "Anonymous",
                    Notification.NotificationID,
                    DateTime.UtcNow);


                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating Notification by user {User}. {ex.Message}",
                    0,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);
                return Page();
            }
        }

    }
}
