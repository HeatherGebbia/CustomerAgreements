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
}
