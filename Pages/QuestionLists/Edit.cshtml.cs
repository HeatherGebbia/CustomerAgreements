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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionlist =  await _context.QuestionLists.FirstOrDefaultAsync(m => m.QuestionListID == id);
            if (questionlist == null)
            {
                return NotFound();
            }
            QuestionList = questionlist;
           
            return Page();
        }
                
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(QuestionList).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving list item by user {User}: {Message}",
                   User.Identity?.Name ?? "Anonymous",
                   ex.Message);

                return Page();
            }

            return RedirectToPage("/Question/Edit", new { id = QuestionList.QuestionID });
        }

        private bool QuestionListExists(int id)
        {
            return _context.QuestionLists.Any(e => e.QuestionListID == id);
        }
    }
}
