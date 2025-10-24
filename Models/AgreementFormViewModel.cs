using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CustomerAgreements.Models
{
    public class AgreementFormViewModel
    {
        public Agreement Agreement { get; set; } = new();
        [ValidateNever]
        public Questionnaire Questionnaire { get; set; } = new();
        public Customer Customer { get; set; } = new();
    }
}
