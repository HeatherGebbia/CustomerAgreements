using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomerAgreements.Helpers
{
    public static class AnswerTypeHelper
    {
        public static List<SelectListItem> GetAnswerTypeOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "TextBox", Text = "Textbox" },
                new SelectListItem { Value = "MultiTextBox", Text = "Multi-line Textbox" },
                new SelectListItem { Value = "RadioButton", Text = "Radio Button List" },
                new SelectListItem { Value = "CheckBox", Text = "Checkbox List" },
                new SelectListItem { Value = "DropDown", Text = "Drop Down List" },
                new SelectListItem { Value = "Date", Text = "Date" },
                new SelectListItem { Value = "SingleCheckBox", Text = "Single Checkbox" }
            };
        }
    }
}
