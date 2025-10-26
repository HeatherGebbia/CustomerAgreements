using Azure.Core;
using CustomerAgreements.Data;
using CustomerAgreements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CustomerAgreements.Services
{
    public class AgreementResponseService
    {
        private readonly ApplicationDbContext _context;

        public AgreementResponseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveAnswersFromFormAsync(int questionnaireId, Agreement agreement, IFormCollection form, Questionnaire questionnaire)
        {
            if (agreement.Questionnaire?.Sections == null)
                return;

            foreach (var section in agreement.Questionnaire.Sections)
            {
                if (section.Questions == null) continue;

                foreach (var question in section.Questions)
                {
                    string fieldName = $"question_{question.QuestionID}";
                    string? userInput = form[fieldName];

                    // Skip unanswered non-required questions
                    if (string.IsNullOrWhiteSpace(userInput) && !question.IsRequired)
                        continue;

                    var answer = new Answer
                    {
                        AgreementID = agreement.AgreementID,
                        QuestionnaireID = questionnaireId,
                        SectionID = question.SectionID,
                        QuestionID = question.QuestionID
                    };

                    // Handle based on type
                    if (question.AnswerType == "Date")
                    {
                        if (DateTime.TryParse(userInput, out var parsedDate))
                            answer.DateAnswer = parsedDate;
                        _context.Answers.Add(answer);
                        await _context.SaveChangesAsync();
                    }
                    else if (question.AnswerType.Contains("List", StringComparison.OrdinalIgnoreCase))
                    {
                        var selectedValues = form[$"question_{question.QuestionID}"];

                        if (question.AnswerType.Contains("Radio") || question.AnswerType.Contains("Drop Down"))
                        {
                            answer.Text = selectedValues.ToString();

                            var selectedListItem = question.QuestionLists
                                .FirstOrDefault(ql => ql.ListValue == answer.Text);

                            if (selectedListItem != null)
                            {
                                answer.QuestionListID = selectedListItem.QuestionListID;

                                _context.Answers.Add(answer);
                                await _context.SaveChangesAsync(); 

                                if (selectedListItem.Conditional)
                                    await SaveDependentAnswersAsync(question, selectedListItem, answer, questionnaireId, agreement, form);
                            }
                        }
                        else if (question.AnswerType.Contains("Checkbox"))
                        {
                            foreach (var val in selectedValues)
                            {
                                var selectedListItem = question.QuestionLists.FirstOrDefault(ql => ql.ListValue == val);
                                if (selectedListItem == null) continue;

                                var answerItem = new Answer
                                {
                                    AgreementID = agreement.AgreementID,
                                    QuestionnaireID = questionnaireId,
                                    SectionID = question.SectionID,
                                    QuestionID = question.QuestionID,
                                    QuestionListID = selectedListItem.QuestionListID,
                                    Text = val
                                };

                                _context.Answers.Add(answerItem);
                                await _context.SaveChangesAsync(); // we need AnswerID

                                if (selectedListItem.Conditional)
                                    await SaveDependentAnswersAsync(question, selectedListItem, answerItem, questionnaireId, agreement, form);
                            }
                        }
                    }
                    else
                    {
                        answer.Text = userInput;
                        _context.Answers.Add(answer);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task SaveDependentAnswersAsync(Question parentQuestion, QuestionList selectedListItem, Answer parentAnswer, int questionnaireId, Agreement agreement, IFormCollection form)
        {
            foreach (var dep in selectedListItem.DependentQuestions)
            {
                var depInput = form[$"dependentQuestion_{dep.DependentQuestionID}"];

                // Skip empty responses unless required
                if (string.IsNullOrWhiteSpace(depInput) && !dep.IsRequired)
                    continue;

                var depAnswer = new DependentAnswer
                {
                    AgreementID = agreement.AgreementID,
                    QuestionnaireID = questionnaireId,
                    SectionID = parentQuestion.SectionID,
                    QuestionID = parentQuestion.QuestionID,
                    QuestionListID = selectedListItem.QuestionListID,
                    DependentQuestionID = dep.DependentQuestionID,
                    AnswerID = parentAnswer.AnswerID 
                };

                if (dep.DependentAnswerType == "Date" &&
                    DateTime.TryParse(depInput, out var parsed))
                {
                    depAnswer.DateAnswer = parsed;
                }
                else
                {
                    depAnswer.Answer = depInput;
                }

                _context.DependentAnswers.Add(depAnswer);
            }

            await _context.SaveChangesAsync();
        }
    }
}
