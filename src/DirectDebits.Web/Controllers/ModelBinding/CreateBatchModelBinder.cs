using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Newtonsoft.Json;
using DirectDebits.ViewModels.Batches;

namespace DirectDebits.Controllers.ModelBinding
{
    public class CreateBatchModelBinder : DefaultModelBinder, IModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            CreateBatchViewModel model = new CreateBatchViewModel();

            string date = GetValue(bindingContext, "date");

            DateTime processDate;
            var isValidDate = DateTime.TryParse(date, out processDate);

            if (isValidDate)
            {
                model.ProcessDate = processDate;
            }
            else
            {
                model.ProcessDate = null;
            }

            string json = GetValue(bindingContext, "json");
            model.Accounts = JsonConvert.DeserializeObject<AccountViewModel[]>(json);

            var validationResults = new List<dynamic>
            {
                from prop in TypeDescriptor.GetProperties(model).Cast<PropertyDescriptor>()
                from attribute in prop.Attributes.OfType<ValidationAttribute>()
                where !attribute.IsValid(prop.GetValue(model))
                select new { Propertie = prop.Name, ErrorMessage = attribute.FormatErrorMessage(string.Empty) }
            };

            // DataAnnotation Validation
            foreach (var account in model.Accounts)
            {
                validationResults.Add(from prop in TypeDescriptor.GetProperties(account).Cast<PropertyDescriptor>()
                         from attribute in prop.Attributes.OfType<ValidationAttribute>()
                         where !attribute.IsValid(prop.GetValue(account))
                         select new { Propertie = prop.Name, ErrorMessage = attribute.FormatErrorMessage(string.Empty) });

                foreach (var invoice in account.Invoices)
                {
                    validationResults.Add(from prop in TypeDescriptor.GetProperties(invoice).Cast<PropertyDescriptor>()
                             from attribute in prop.Attributes.OfType<ValidationAttribute>()
                             where !attribute.IsValid(prop.GetValue(invoice))
                             select new { Propertie = prop.Name, ErrorMessage = attribute.FormatErrorMessage(string.Empty) });
                }
            }

            // Add the ValidationResults to the ModelState
            foreach (var validationResult in validationResults)
            {
                foreach (var validationResultItem in validationResult)
                {
                    bindingContext.ModelState.AddModelError(validationResultItem.Propertie, validationResultItem.ErrorMessage);
                }
            }

            return model;
        }

        /// <summary>
        /// Gets a parameter from the Value Provider using a specific key
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetValue(ModelBindingContext context, string key)
        {
            ValueProviderResult vpr = context.ValueProvider.GetValue(key);
            return vpr?.AttemptedValue;
        }
    }
}