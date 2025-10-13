using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CustomerAgreements.Data;
using CustomerAgreements.Models;

namespace CustomerAgreements.Pages.Sections
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
        public Section Section { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int sectionId)
        {
            try
            {
                _logger.LogInformation($"User Viewed Sections edit page",
                            User.Identity?.Name ?? "Anonymous",
                            0,
                            DateTime.UtcNow);

                var section = await _context.Sections.FirstOrDefaultAsync(m => m.SectionID == sectionId);
                if (section == null)
                {
                    return NotFound();
                }
                Section = section;
                ViewData["QuestionnaireID"] = new SelectList(_context.Questionnaires, "QuestionnaireID", "QuestionnaireName");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading section {sectionId} for edit", sectionId);
                return StatusCode(500, "An error occurred while loading the section.");
            }
        }
                
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Section).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SectionExists(Section.SectionID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/Questionnaires/Edit", new { id = Section.QuestionnaireID });
        }

        private bool SectionExists(int id)
        {
            return _context.Sections.Any(e => e.SectionID == id);
        }
    }
}
