using CustomerAgreements.Data;
using CustomerAgreements.Helpers;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerAgreements.Pages.DependentQuestionLists
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
        public DependentQuestionList DependentQuestionList { get; set; } = new DependentQuestionList();

        public async Task<IActionResult> OnGetAsync(int dependentQuestionListID)
        {
            _logger.LogInformation($"User Viewed Dependent Question Lists edit page",
                            User.Identity?.Name ?? "Anonymous",
                            0,
                            DateTime.UtcNow);

            DependentQuestionList = await _context.DependentQuestionLists
                .FirstOrDefaultAsync(ql => ql.DependentQuestionListID == dependentQuestionListID);

            if (DependentQuestionList == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int dependentQuestionId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }            

            try
            {
                _context.Attach(DependentQuestionList).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {User} updated dependent list item {Id}",
                    User.Identity?.Name ?? "Anonymous",
                    DependentQuestionList.DependentQuestionListID);

                return RedirectToPage("/DependentQuestions/Edit", new
                {
                    dependentQuestionId = dependentQuestionId
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving dependent list item by user {User}: {Message}",
                   User.Identity?.Name ?? "Anonymous",
                   ex.Message);

                return Page();
            }
        }
    }
}
