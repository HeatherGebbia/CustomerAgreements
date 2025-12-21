using CustomerAgreements.Contracts.Dtos;
using CustomerAgreements.Data;
using CustomerAgreements.Models;
using CustomerAgreements.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerAgreements.Api.Controllers;

[ApiController]
[Route("api/agreements")]
public class AgreementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AgreementResponseService _agreementService;

    public AgreementsController(ApplicationDbContext context, AgreementResponseService agreementService)
    {
        _context = context;
        _agreementService = agreementService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateAgreementResponseDto>> Create([FromBody] CreateAgreementRequestDto request)
    {
        var isSubmit = string.Equals(request.ActionType, "Submit", StringComparison.OrdinalIgnoreCase);

        // Load questionnaire shape (same as your Create page)
        var questionnaire = await _context.Questionnaires
            .Include(q => q.Sections)
                .ThenInclude(s => s.Questions)
                .ThenInclude(q => q.QuestionLists)
                .ThenInclude(ql => ql.DependentQuestions)
            .FirstAsync(q => q.QuestionnaireID == request.QuestionnaireId);

        // 1) Create customer
        var customer = new Customer
        {
            CompanyName = request.Customer.CompanyName,
            ContactName = request.Customer.ContactName,
            EmailAddress = request.Customer.EmailAddress
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // 2) Create agreement
        var agreement = new Agreement
        {
            QuestionnaireID = request.QuestionnaireId,
            CustomerID = customer.CustomerID,
            CustomerName = customer.CompanyName,
            CustomerEmail = customer.EmailAddress,
            Status = isSubmit ? "Submitted" : "Draft",
            SubmittedDate = isSubmit ? DateTime.UtcNow : null
        };

        _context.Agreements.Add(agreement);
        await _context.SaveChangesAsync();       

        await _agreementService.SaveOrUpdateAnswersFromApiAsync(request.QuestionnaireId, agreement, questionnaire, request.Answers);


        return Ok(new CreateAgreementResponseDto
        {
            AgreementId = agreement.AgreementID,
            Status = agreement.Status
        });
    }

    [HttpGet("{agreementId:int}/edit-payload")]
    public async Task<ActionResult<EditAgreementPayloadDto>> GetEditPayload(int agreementId)
    {
        // 1) Load agreement + customer
        var agreement = await _context.Agreements
            .AsNoTracking()
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.AgreementID == agreementId);

        if (agreement == null)
            return NotFound($"Agreement {agreementId} not found.");

        // 2) Load questionnaire shape
        var questionnaire = await _context.Questionnaires
            .AsNoTracking()
            .Include(q => q.Sections)
                .ThenInclude(s => s.Questions)
                .ThenInclude(q => q.QuestionLists)
                .ThenInclude(ql => ql.DependentQuestions)
            .FirstOrDefaultAsync(q => q.QuestionnaireID == agreement.QuestionnaireID);

        if (questionnaire == null)
            return NotFound($"Questionnaire {agreement.QuestionnaireID} not found.");

        // 3) Load instructions + acknowledgement
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

        // 4) Load existing answers
        var answers = await _context.Answers
            .AsNoTracking()
            .Where(a => a.AgreementID == agreementId)
            .ToListAsync();

        var dependentAnswers = await _context.DependentAnswers
            .AsNoTracking()
            .Where(d => d.AgreementID == agreementId)
            .ToListAsync();

        // 5) Map questionnaire shape to DTO (reuse your existing mapping style)
        var questionnaireDto = new QuestionnaireDto
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
        };

        // 6) Build AnswerDto list (prefill values)
        // Helpful lookup so we know which questions are checkbox lists
        //var questionById = questionnaire.Sections
        //    .SelectMany(s => s.Questions)
        //    .ToDictionary(q => q.QuestionID, q => q);

        var answerDtos = new List<AnswerDto>();

        foreach (var row in answers)
        {
            // Determine question type from questionnaire definition
            //questionById.TryGetValue(row.QuestionID, out var questionDef);

            // Base dto for this saved answer row
            var dto = new AnswerDto
            {
                QuestionId = row.QuestionID,
                QuestionListId = row.QuestionListID
            };

            // Date answers: put yyyy-MM-dd into Value
            if (row.DateAnswer.HasValue)
            {
                dto.Value = row.DateAnswer.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                dto.Value = row.Text; // text, radio/dropdown value, single checkbox true/false, checkbox selection text, etc.
            }

            // Attach dependent answers if this row has a QuestionListID
            if (row.QuestionListID.HasValue)
            {
                var deps = dependentAnswers
                    .Where(d =>
                        d.QuestionListID == row.QuestionListID.Value &&
                        d.QuestionID == row.QuestionID &&
                        d.AgreementID == agreementId)
                    .Select(d => new DependentAnswerDto
                    {
                        DependentQuestionId = d.DependentQuestionID,
                        Value = d.DateAnswer.HasValue
                            ? d.DateAnswer.Value.ToString("yyyy-MM-dd")
                            : d.Answer
                    })
                    .ToList();

                if (deps.Count > 0)
                    dto.DependentAnswers = deps;
            }

            answerDtos.Add(dto);
        }


        // 7) Return payload
        var payload = new EditAgreementPayloadDto
        {
            QuestionnaireId = agreement.QuestionnaireID,
            AgreementId = agreement.AgreementID,
            Instructions = instructions,
            Acknowledgement = acknowledgement,
            Status = agreement.Status ?? "Draft",
            Customer = new CustomerDto
            {
                CompanyName = agreement.Customer?.CompanyName ?? agreement.CustomerName ?? "",
                ContactName = agreement.Customer?.ContactName ?? "",
                EmailAddress = agreement.Customer?.EmailAddress ?? agreement.CustomerEmail ?? ""
            },
            Answers = answerDtos,
            Questionnaire = questionnaireDto
        };

        return Ok(payload);
    }
}
