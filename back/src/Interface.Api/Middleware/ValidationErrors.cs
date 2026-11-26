using System.Text.Json;
using FluentValidation;

namespace Interface.Api.Middleware;

internal static class ValidationErrors
{
    /// <summary>
    /// Groups FluentValidation failures by field, with field names in the same camelCase the JSON body uses
    /// (e.g. "Allowances[0].Amount" becomes "allowances[0].amount") so the UI can show each error next to its input.
    /// </summary>
    public static Dictionary<string, string[]> ToDictionary(ValidationException exception) =>
        exception.Errors
            .GroupBy(failure => ToCamelCasePath(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

    private static string ToCamelCasePath(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
