using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CustomerAgreements.TagHelpers
{
    [HtmlTargetElement("input", Attributes = ForAttr)]
    public class ValidationInputTagHelper : TagHelper
    {
        private const string ForAttr = "asp-for";

        [HtmlAttributeName(ForAttr)]
        public ModelExpression For { get; set; } = default!;

        // Inject the current ViewContext so we can read ModelState
        [ViewContext]
        public ViewContext ViewContext { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var fieldName = For.Name; 

            if (ViewContext.ViewData.ModelState.TryGetValue(fieldName, out var entry) &&
                entry.Errors.Count > 0)
            {
                var existing = output.Attributes.ContainsName("class")
                    ? output.Attributes["class"].Value?.ToString()
                    : string.Empty;

                var newClass = string.IsNullOrWhiteSpace(existing)
                    ? "is-invalid"
                    : $"{existing} is-invalid";

                output.Attributes.SetAttribute("class", newClass);
            }
        }
    }
}
