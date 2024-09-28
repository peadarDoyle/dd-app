using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DirectDebits.Attributes.Validation
{
    public sealed class IsLessThan : ValidationAttribute, IClientValidatable
    {
        private readonly string testedPropertyName;

        public IsLessThan(string testedPropertyName)
        {
            this.testedPropertyName = testedPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // when the value is null we return "Success" because another validation attribute should
            // be used to capture the null result
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var propertyTestedInfo = validationContext.ObjectType.GetProperty(testedPropertyName);
            if (propertyTestedInfo == null)
            {
                return new ValidationResult($"unknown property {testedPropertyName}");
            }

            var propertyTestedValue = propertyTestedInfo.GetValue(validationContext.ObjectInstance, null);

            // Compare values
            if ((int)value < (int)propertyTestedValue)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = this.ErrorMessageString,
                ValidationType = "isdateafter"
            };
            rule.ValidationParameters["propertytested"] = testedPropertyName;
            yield return rule;
        }
    }
}