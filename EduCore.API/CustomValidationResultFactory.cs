using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace EduCore.API;

public class CustomValidationResultFactory : IFluentValidationAutoValidationResultFactory
{
    public Task<IActionResult?> CreateActionResult(ActionExecutingContext context,
        ValidationProblemDetails validationProblemDetails,
        IDictionary<IValidationContext, ValidationResult> validationResults)
    {
        // extract errors 
        var failures = validationResults.Values
            .SelectMany(x => x.Errors)
            .ToList();


        if (!failures.Any())
        {
            failures = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new ValidationFailure(x.Key, e.ErrorMessage)))
                .ToList();
        }

        throw new ValidationException(failures);
    }
}