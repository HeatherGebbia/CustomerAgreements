namespace CustomerAgreements.Models
{
    public class AgreementFormViewModel
    {
        public Agreement Agreement { get; set; } = new();
        public Questionnaire Questionnaire { get; set; } = new();
    }
}
