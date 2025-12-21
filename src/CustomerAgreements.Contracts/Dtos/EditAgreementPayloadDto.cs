namespace CustomerAgreements.Contracts.Dtos;

public class EditAgreementPayloadDto
{
    public int QuestionnaireId { get; set; }
    public int AgreementId { get; set; }
    public string Instructions { get; set; } = "";
    public string Acknowledgement { get; set; } = "";
    public string Status { get; set; } = "Draft"; 
    public CustomerDto Customer { get; set; } = new();
    public List<AnswerDto> Answers { get; set; } = new();

    public QuestionnaireDto Questionnaire { get; set; } = new();
}