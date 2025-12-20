using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomerAgreements.Pages.Notifications
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
        public Notification Notification { get; set; } = new Notification();

        public void OnGet()
        {
            try
            {
                Notification = new Notification
                {
                    NotificationType = "Text"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading new Notification by user {User}. {ex.Message}",
                    -1,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);
            }            
        }

        public async Task<IActionResult> OnPostSave()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Notification.LastUpdatedDate = DateTime.UtcNow;
                Notification.Active = true;
                Notification.NotificationKey = Notification.NotificationName.Replace(" ", "");
                _context.Notifications.Add(Notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {User} created new Notification",
                    User.Identity?.Name ?? "Anonymous",
                    Notification.NotificationID,
                    DateTime.UtcNow);

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating Notification. {ex.Message}",
                    Notification.NotificationName,
                    User.Identity?.Name ?? "Anonymous",
                    DateTime.UtcNow);

                ModelState.AddModelError("", "Unable to save Notification. See logs for details.");
                return Page();
            }
        }
    }
}
