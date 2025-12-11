using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CustomerAgreements.Pages.Lists
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

        // Route + form key for this list "group"
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string ListName { get; set; } = string.Empty;

        // Values for the listbox
        public List<SelectListItem> ListValues { get; set; } = new();

        // For Add/Update textbox
        [BindProperty]
        public string NewValue { get; set; } = string.Empty;

        // For multi-select delete
        [BindProperty]
        public List<int>? SelectedIds { get; set; } = new();

        // For tracking which item is being edited
        [BindProperty]
        public int? EditId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("User Viewed Lists edit page {User} {ListId} {UtcNow}",
                    User.Identity?.Name ?? "Anonymous",
                    id,
                    DateTime.UtcNow);

                Id = id;

                var baseRow = await _context.Lists.FindAsync(id);
                if (baseRow == null)
                {
                    return NotFound();
                }

                ListName = baseRow.ListName;
                await LoadValuesAsync(ListName);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading List {Id} for edit", id);
                return StatusCode(500, "An error occurred while loading the List.");
            }
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            // Re-hydrate based on route/form Id
            var baseRow = await _context.Lists.FindAsync(Id);
            if (baseRow == null)
            {
                return NotFound();
            }

            ListName = baseRow.ListName;

            if (string.IsNullOrWhiteSpace(NewValue))
            {
                ModelState.AddModelError(nameof(NewValue), "Please enter a value to add.");
                await LoadValuesAsync(ListName);
                return Page();
            }

            var newRow = new CustomerAgreements.Models.List
            {
                ListName = ListName,
                ListValue = NewValue.Trim()
            };

            _context.Lists.Add(newRow);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = Id });
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var baseRow = await _context.Lists.FindAsync(Id);
            if (baseRow == null)
            {
                return NotFound();
            }

            ListName = baseRow.ListName;

            if (SelectedIds == null || !SelectedIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select one or more values to delete.");
                await LoadValuesAsync(ListName);
                return Page();
            }            

            var rows = await _context.Lists
                .Where(l => l.ListName == ListName && SelectedIds.Contains(l.ListID))
                .ToListAsync();

            _context.Lists.RemoveRange(rows);
            await _context.SaveChangesAsync();

            // Find a remaining row with this ListName to use as the new anchor
            var newAnchor = await _context.Lists
                .Where(l => l.ListName == ListName)
                .OrderBy(l => l.ListID)
                .FirstOrDefaultAsync();

            if (newAnchor == null)
            {
                // No rows left for this ListName — go back to the List Index
                return RedirectToPage("/Lists/Index");
            }

            return RedirectToPage(new { id = newAnchor.ListID });
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var baseRow = await _context.Lists.FindAsync(Id);
            if (baseRow == null)
            {
                return NotFound();
            }

            ListName = baseRow.ListName;

            if (EditId == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a single value to edit.");
                await LoadValuesAsync(ListName);
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewValue))
            {
                ModelState.AddModelError(nameof(NewValue), "Please enter the updated value.");
                await LoadValuesAsync(ListName);
                return Page();
            }

            var row = await _context.Lists
                .FirstOrDefaultAsync(l => l.ListID == EditId.Value && l.ListName == ListName);

            if (row == null)
            {
                ModelState.AddModelError(string.Empty, "Selected value was not found.");
                await LoadValuesAsync(ListName);
                return Page();
            }

            row.ListValue = NewValue.Trim();
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = Id });
        }

        private async Task LoadValuesAsync(string listName)
        {
            ListValues = await _context.Lists
                .Where(l => l.ListName == listName)
                .OrderBy(l => l.ListValue)
                .Select(l => new SelectListItem
                {
                    Value = l.ListID.ToString(),
                    Text = l.ListValue
                })
                .ToListAsync();
        }
    }
}
