using CustomerAgreements.Data;
using CustomerAgreements.Models;
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

        /// <summary>
        /// Saves or updates answers and dependent answers for a given agreement.
        /// Works for both Create and Edit pages.
        /// </summary>
        public async Task SaveOrUpdateAnswersFromFormAsync(int questionnaireId, Agreement agreement,
            IFormCollection form, Questionnaire questionnaire)
        {
            if (questionnaire?.Sections == null)
                return;

            foreach (var section in questionnaire.Sections)
            {
                if (section.Questions == null) continue;

                foreach (var question in section.Questions)
                {
                    string fieldName = $"question_{question.QuestionID}";
                    string? userInput = form[fieldName];

                    bool isSingleCheckbox = question.AnswerType == "Single Checkbox";

                    // Skip empty + not required for everything EXCEPT Single Checkbox
                    if (string.IsNullOrWhiteSpace(userInput) && !question.IsRequired && !isSingleCheckbox)
                        continue;

                    // Try to find an existing answer (Edit mode)
                    var existingAnswer = await _context.Answers
                        .FirstOrDefaultAsync(a =>
                            a.AgreementID == agreement.AgreementID &&
                            a.QuestionID == question.QuestionID);

                    var answer = existingAnswer ?? new Answer
                    {
                        AgreementID = agreement.AgreementID,
                        QuestionnaireID = questionnaireId,
                        SectionID = question.SectionID,
                        QuestionID = question.QuestionID
                    };

                    // === Handle based on answer type ===
                    if (question.AnswerType == "Date")
                    {
                        if (DateTime.TryParse(userInput, out var parsedDate))
                            answer.DateAnswer = parsedDate;
                        else
                            answer.DateAnswer = null;

                        if (existingAnswer == null)
                            _context.Answers.Add(answer);

                        await _context.SaveChangesAsync();
                    }
                    else if (question.AnswerType.Contains("List", StringComparison.OrdinalIgnoreCase))
                    {
                        var selectedValues = form[$"question_{question.QuestionID}"];

                        // Radio or Drop Down
                        if (question.AnswerType.Contains("Radio") || question.AnswerType.Contains("Drop Down"))
                        {
                            answer.Text = selectedValues.ToString();

                            var selectedListItem = question.QuestionLists
                                .FirstOrDefault(ql => ql.ListValue == answer.Text);

                            if (selectedListItem != null)
                            {
                                answer.QuestionListID = selectedListItem.QuestionListID;

                                if (existingAnswer == null)
                                    _context.Answers.Add(answer);

                                await _context.SaveChangesAsync(); // ensure AnswerID exists

                                // Handle dependent questions
                                if (selectedListItem.Conditional)
                                    await SaveOrUpdateDependentAnswersAsync(question, selectedListItem, answer, questionnaireId, agreement, form);
                            }
                        }
                        // Checkbox List (can have multiple selections)
                        else if (question.AnswerType.Contains("Checkbox"))
                        {
                            // Remove old answers first
                            var oldAnswers = _context.Answers
                                .Where(a => a.AgreementID == agreement.AgreementID && a.QuestionID == question.QuestionID);
                            _context.Answers.RemoveRange(oldAnswers);
                            await _context.SaveChangesAsync();

                            foreach (var val in selectedValues)
                            {
                                var selectedListItem = question.QuestionLists.FirstOrDefault(ql => ql.ListValue == val);
                                if (selectedListItem == null) continue;

                                var newAnswer = new Answer
                                {
                                    AgreementID = agreement.AgreementID,
                                    QuestionnaireID = questionnaireId,
                                    SectionID = question.SectionID,
                                    QuestionID = question.QuestionID,
                                    QuestionListID = selectedListItem.QuestionListID,
                                    Text = val
                                };

                                _context.Answers.Add(newAnswer);
                                await _context.SaveChangesAsync(); 

                                if (selectedListItem.Conditional)
                                    await SaveOrUpdateDependentAnswersAsync(question, selectedListItem, newAnswer, questionnaireId, agreement, form);
                            }
                        }
                    }
                    else if (question.AnswerType == "Single Checkbox")
                    {
                        // Checkbox is only "present" in the form if checked
                        var isChecked = !string.IsNullOrEmpty(userInput);

                        // Store a clean true/false string
                        answer.Text = isChecked ? "true" : "false";

                        if (existingAnswer == null)
                            _context.Answers.Add(answer);

                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Simple text or number answers
                        answer.Text = userInput;

                        if (existingAnswer == null)
                            _context.Answers.Add(answer);

                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Saves or updates dependent answers linked to a parent Answer.
        /// </summary>
        private async Task SaveOrUpdateDependentAnswersAsync(Question parentQuestion, QuestionList selectedListItem, Answer parentAnswer, int questionnaireId, Agreement agreement,
            IFormCollection form)
        {
            foreach (var dep in selectedListItem.DependentQuestions)
            {
                var depInput = form[$"dependentQuestion_{dep.DependentQuestionID}"];

                // Find existing dependent answer if any
                var existingDep = await _context.DependentAnswers.FirstOrDefaultAsync(d =>
                    d.AgreementID == agreement.AgreementID &&
                    d.QuestionListID == selectedListItem.QuestionListID &&
                    d.DependentQuestionID == dep.DependentQuestionID);

                // Skip empty responses unless required
                if (string.IsNullOrWhiteSpace(depInput) && !dep.IsRequired)
                {
                    if (existingDep != null)
                        _context.DependentAnswers.Remove(existingDep);
                    continue;
                }

                var depAnswer = existingDep ?? new DependentAnswer
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
                    depAnswer.Answer = null;
                }
                else
                {
                    depAnswer.Answer = depInput;
                    depAnswer.DateAnswer = null;
                }

                if (existingDep == null)
                    _context.DependentAnswers.Add(depAnswer);

                await _context.SaveChangesAsync();
            }
        }
    }
}
