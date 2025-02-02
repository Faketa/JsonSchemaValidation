using JsonSchemaValidation.Models;
using JsonSchemaValidation.ValidationRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSchemaValidation.Test
{
    [TestFixture]
    public class ValidationRulesTest
    {
        private LengthValidationRule _lengthValidationRule;
        private MandatoryValidationRule _mandatoryValidationRule;

        [SetUp]
        public void Setup()
        {
            _lengthValidationRule = new LengthValidationRule();
            _mandatoryValidationRule = new MandatoryValidationRule();
        }

        [Test]
        public void LengthValidationRule_ValidLength_ShouldReturnSuccess()
        {
            var testLength = 3;
            var schemaField = new SchemaField(testLength, false, null, null);
            var result = _lengthValidationRule.Validate("age", "20", schemaField);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void LengthValidationRule_ExceedsMaxLength_ShouldReturnFailure()
        {
            var testLength = 3;
            var schemaField = new SchemaField(testLength, false, null, null);
            var result = _lengthValidationRule.Validate("age", "3000", schemaField);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual($"Field exceeds maximum length of {testLength}.", result.ErrorMessage);
        }

        [Test]
        public void MandatoryValidationRule_MissingMandatoryValue_ShouldReturnFailure()
        {
            var schemaField = new SchemaField(0, true, null, null);
            var result = _mandatoryValidationRule.Validate("name", "", schemaField);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Field is mandatory but missing or empty.", result.ErrorMessage);
        }

        [Test]
        public void MandatoryValidationRule_NonMandatoryEmptyValue_ShouldReturnSuccess()
        {
            var schemaField = new SchemaField(0, false, null, null);
            var result = _mandatoryValidationRule.Validate("name", "", schemaField);

            Assert.IsTrue(result.IsValid);
        }
    }
}
