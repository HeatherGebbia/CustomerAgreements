namespace CustomerAgreements.Contracts.Dtos;

public class CreateAgreementPayloadDto
{
    public int QuestionnaireId { get; set; }
    public string Instructions { get; set; } = "";
    public string Acknowledgement { get; set; } = "";

    public QuestionnaireDto Questionnaire { get; set; } = new();
}

public class QuestionnaireDto
{
    public int QuestionnaireId { get; set; }
    public string QuestionnaireName { get; set; } = "";
    public string Status { get; set; } = "";

    public List<SectionDto> Sections { get; set; } = new();
}

public class SectionDto
{
    public int SectionId { get; set; }
    public int QuestionnaireId { get; set; }

    public string Text { get; set; } = "";
    public int SortOrder { get; set; }

    public bool IncludeInstructions { get; set; }
    public string? Instructions { get; set; }

    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public int Id { get; set; }                 // maps to Question.ID (PK)
    public int QuestionId { get; set; }         // maps to Question.QuestionID
    public int QuestionnaireId { get; set; }
    public int SectionId { get; set; }

    public string QuestionTitle { get; set; } = "";
    public string? Text { get; set; }           // the column named "Question"
    public string QuestionText { get; set; } = "";
    public string AnswerType { get; set; } = "";

    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }

    public List<QuestionListDto> QuestionLists { get; set; } = new();
}

public class QuestionListDto
{
    public int QuestionListId { get; set; }
    public int QuestionnaireId { get; set; }
    public int SectionId { get; set; }

    public int QuestionId { get; set; }   // IMPORTANT: this is the business QuestionID (library id)
    public string ListValue { get; set; } = "";

    public bool Conditional { get; set; }
    public int SortOrder { get; set; }

    public bool SendEmail { get; set; }
    public int? NotificationId { get; set; }

    public List<DependentQuestionDto> DependentQuestions { get; set; } = new();
}

public class DependentQuestionDto
{
    public int DependentQuestionId { get; set; }

    public int QuestionnaireId { get; set; }
    public int SectionId { get; set; }
    public int QuestionId { get; set; }       // business QuestionID
    public int QuestionListId { get; set; }

    public string DependentQuestionTitle { get; set; } = "";
    public string? Text { get; set; }
    public string DependentQuestionText { get; set; } = "";
    public string DependentAnswerType { get; set; } = "";

    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}
