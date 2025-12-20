
namespace CustomerAgreements.Contracts.Dtos;

public class CreateAgreementRequestDto
{
    public int QuestionnaireId { get; set; }
    public string ActionType { get; set; } = "Draft"; // "Draft" or "Submit"

    public CustomerDto Customer { get; set; } = new();
    public List<AnswerDto> Answers { get; set; } = new();
}

public class CustomerDto
{
    public string CompanyName { get; set; } = "";
    public string ContactName { get; set; } = "";
    public string EmailAddress { get; set; } = "";
}

public class AnswerDto
{
    public int QuestionId { get; set; }                 // business QuestionID
    public string? Value { get; set; }                  // single value
    public List<string>? Values { get; set; }           // checkbox list values
    public int? QuestionListId { get; set; }            // radio/dropdown selection
    public List<DependentAnswerDto>? DependentAnswers { get; set; }
}

public class DependentAnswerDto
{
    public int DependentQuestionId { get; set; }
    public string? Value { get; set; }
}
