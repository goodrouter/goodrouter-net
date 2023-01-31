internal static class StringUtility
{
    public static int FindCommonPrefixLength(
        string stringLeft,
        string stringRight
    )
    {
        var length = Math.Min(stringLeft.Length, stringRight.Length);
        int index;
        for (index = 0; index < length; index++)
        {
            var charLeft = stringLeft[index];
            var charRight = stringRight[index];
            if (charLeft != charRight)
            {
                break;
            }
        }
        return index;
    }

    public static IEnumerable<string> ParsePlaceholders(
        string template,
        string parameterPrefix = "{",
        string parameterSuffix = "}"
    )
    {
        var offsetIndex = 0;

        for (; ; )
        {
            var prefixIndex = template.IndexOf(parameterPrefix, offsetIndex);
            if (prefixIndex < 0)
            {
                break;
            }
            var suffixIndex = template.IndexOf(parameterSuffix, prefixIndex);
            if (suffixIndex < 0)
            {
                break;
            }

            yield return template.Substring(offsetIndex, prefixIndex - offsetIndex);
            offsetIndex = prefixIndex + parameterPrefix.Length;

            yield return template.Substring(offsetIndex, suffixIndex - offsetIndex);
            offsetIndex = suffixIndex + parameterSuffix.Length;
        }

        yield return template.Substring(offsetIndex);
    }
}
