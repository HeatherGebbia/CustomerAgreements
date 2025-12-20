using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CustomerAgreements.Models;
using Xunit;

namespace CustomerAgreements.Tests
{
    public class SectionModelValidationTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Should_Fail_When_IncludeInstructions_True_And_Instructions_Empty()
        {
            // Arrange
            var section = new Section
            {
                Text = "Test Section",
                SortOrder = 1,
                IncludeInstructions = true,
                Instructions = null
            };

            // Act
            var results = ValidateModel(section);

            // Assert
            Assert.Contains(results, v => v.ErrorMessage!.Contains("Instructions are required"));
        }

        [Fact]
        public void Should_Pass_When_IncludeInstructions_False()
        {
            var section = new Section
            {
                Text = "Test Section",
                SortOrder = 1,
                IncludeInstructions = false,
                Instructions = null
            };

            var results = ValidateModel(section);

            Assert.Empty(results); // no errors
        }

        [Fact]
        public void Should_Pass_When_IncludeInstructions_True_And_Instructions_Filled()
        {
            var section = new Section
            {
                Text = "Test Section",
                SortOrder = 1,
                IncludeInstructions = true,
                Instructions = "Some instructions"
            };

            var results = ValidateModel(section);

            Assert.Empty(results); // no errors
        }
    }
}
