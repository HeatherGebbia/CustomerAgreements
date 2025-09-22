using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomerAgreements.Helpers
{
    public static class AnswerTypeHelper
    {
        public static List<SelectListItem> GetAnswerTypeOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Single-line Textbox", Text = "Textbox" },
                new SelectListItem { Value = "Multi-line Textbox", Text = "Multi-line Textbox" },
                new SelectListItem { Value = "Radio Button List", Text = "Radio Button List" },
                new SelectListItem { Value = "Checkbox List", Text = "Checkbox List" },
                new SelectListItem { Value = "Drop Down List", Text = "Drop Down List" },
                new SelectListItem { Value = "Date", Text = "Date" },
                new SelectListItem { Value = "Single Checkbox", Text = "Single Checkbox" }
            };
        }
    }
}
