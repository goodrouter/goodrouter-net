using System.Text.RegularExpressions;

internal static class TemplateUtility
{

    public static IEnumerable<string> ParseTemplateParts(
        string routeTemplate,
        Regex parameterPlaceholderRE
    )
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<(string, string?)> ParseTemplatePairs(
        string template,
        Regex parameterPlaceholderRE
    )
    {
        throw new NotImplementedException();
    }
}
