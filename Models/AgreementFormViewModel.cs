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
        public Dictionary<int, Answer> AnswersByQuestionId => Agreement?.Answers?
            .GroupBy(a => a.QuestionID)
            .ToDictionary(g => g.Key, g => g.First()) ?? new();
        public Dictionary<int, DependentAnswer> DependentAnswersByQuestionId => Agreement?.Answers?
            .SelectMany(a => a.DependentAnswers)
            .GroupBy(d => d.DependentQuestionID)
            .ToDictionary(g => g.Key, g => g.First()) ?? new();


    }
}
