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

namespace CustomerAgreements.Pages.QuestionLists
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
        public QuestionList QuestionList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id, int questionId, int questionnaireId)
        {
            QuestionList = await _context.QuestionLists
            .FirstOrDefaultAsync(q => q.QuestionListID == id
                                    && q.QuestionID == questionId
                                   && q.QuestionnaireID == questionnaireId);

            if (QuestionList == null)
            {
                return NotFound();
            }

            return Page();
        }
                
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }            

            try
            {
                var listItem = await _context.QuestionLists
                .FirstOrDefaultAsync(q => q.QuestionListID == id);

                if (listItem == null)
                {
                    return NotFound();
                }

                listItem.ListValue = QuestionList.ListValue;
                listItem.SortOrder = QuestionList.SortOrder;
                listItem.Conditional = QuestionList.Conditional;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving list item by user {User}: {Message}",
                   User.Identity?.Name ?? "Anonymous",
                   ex.Message);

                return Page();
            }

            return RedirectToPage("/Questions/Edit", new { id = QuestionList.QuestionID, questionnaireId = QuestionList.QuestionnaireID });
        }

        private bool QuestionListExists(int id)
        {
            return _context.QuestionLists.Any(e => e.QuestionListID == id);
        }
    }
}
