using CustomerAgreements.Contracts.Dtos;
using CustomerAgreements.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerAgreements.Api.Controllers;

[ApiController]
[Route("api/questionnaires")]
public class QuestionnairesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public QuestionnairesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET api/questionnaires/{questionnaireId}/create-payload
    [HttpGet("{questionnaireId:int}/create-payload")]
    public async Task<ActionResult<CreateAgreementPayloadDto>> GetCreatePayload(int questionnaireId)
    {
        var questionnaire = await _context.Questionnaires
            .AsNoTracking()
            .Include(q => q.Sections)
                .ThenInclude(s => s.Questions)
                .ThenInclude(q => q.QuestionLists)
                .ThenInclude(ql => ql.DependentQuestions)
            .FirstOrDefaultAsync(q => q.QuestionnaireID == questionnaireId);

        if (questionnaire == null)
            return NotFound($"Questionnaire {questionnaireId} not found.");

        var instructions = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.NotificationKey == "Instructions")
            .Select(n => n.Text)
            .FirstOrDefaultAsync() ?? "No instructions available.";

        var acknowledgement = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.NotificationKey == "Acknowledgement")
            .Select(n => n.Text)
            .FirstOrDefaultAsync() ?? "No acknowledgement available.";

        var dto = new CreateAgreementPayloadDto
        {
            QuestionnaireId = questionnaireId,
            Instructions = instructions,
            Acknowledgement = acknowledgement,
            Questionnaire = new QuestionnaireDto
            {
                QuestionnaireId = questionnaire.QuestionnaireID,
                QuestionnaireName = questionnaire.QuestionnaireName,
                Status = questionnaire.Status,

                Sections = questionnaire.Sections
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new SectionDto
                    {
                        SectionId = s.SectionID,
                        QuestionnaireId = s.QuestionnaireID,
                        Text = s.Text,
                        SortOrder = s.SortOrder,
                        IncludeInstructions = s.IncludeInstructions,
                        Instructions = s.Instructions,

                        Questions = s.Questions
                            .OrderBy(q => q.SortOrder)
                            .Select(q => new QuestionDto
                            {
                                Id = q.ID,
                                QuestionId = q.QuestionID,
                                QuestionnaireId = q.QuestionnaireID,
                                SectionId = q.SectionID,
                                QuestionTitle = q.QuestionTitle,
                                Text = q.Text,
                                QuestionText = q.QuestionText,
                                AnswerType = q.AnswerType,
                                IsRequired = q.IsRequired,
                                SortOrder = q.SortOrder,

                                QuestionLists = q.QuestionLists
                                    .OrderBy(ql => ql.SortOrder)
                                    .Select(ql => new QuestionListDto
                                    {
                                        QuestionListId = ql.QuestionListID,
                                        QuestionnaireId = ql.QuestionnaireID,
                                        SectionId = ql.SectionID,
                                        QuestionId = ql.QuestionID,
                                        ListValue = ql.ListValue,
                                        Conditional = ql.Conditional,
                                        SortOrder = ql.SortOrder,
                                        SendEmail = ql.SendEmail,
                                        NotificationId = ql.NotificationID,

                                        DependentQuestions = ql.DependentQuestions
                                            .OrderBy(dq => dq.SortOrder)
                                            .Select(dq => new DependentQuestionDto
                                            {
                                                DependentQuestionId = dq.DependentQuestionID,
                                                QuestionnaireId = dq.QuestionnaireID,
                                                SectionId = dq.SectionID,
                                                QuestionId = dq.QuestionID,
                                                QuestionListId = dq.QuestionListID,
                                                DependentQuestionTitle = dq.DependentQuestionTitle,
                                                Text = dq.Text,
                                                DependentQuestionText = dq.DependentQuestionText,
                                                DependentAnswerType = dq.DependentAnswerType,
                                                IsRequired = dq.IsRequired,
                                                SortOrder = dq.SortOrder
                                            })
                                            .ToList()
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            }
        };

        return Ok(dto);
    }
}
