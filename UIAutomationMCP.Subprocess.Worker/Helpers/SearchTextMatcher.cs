using System.Text;

namespace UIAutomationMCP.Subprocess.Worker.Helpers
{
    /// <summary>
    /// Provides cross-property text matching for element searches.
    /// UI Automation's PropertyCondition only supports exact matches, so the SearchElements
    /// substring/fuzzy filtering must be performed in-process against the found elements.
    /// </summary>
    public static class SearchTextMatcher
    {
        /// <summary>
        /// Returns true if <paramref name="candidate"/> matches <paramref name="searchText"/>.
        /// When <paramref name="fuzzyMatch"/> is false, performs a case-insensitive substring match.
        /// When true, normalizes both sides (dropping whitespace and non-alphanumeric characters and
        /// lowercasing) before performing the substring match, so a query like "RichEditBox" also
        /// matches "Rich Edit Box", "rich-edit-box", "rich_edit_box", etc.
        /// </summary>
        public static bool IsMatch(string? candidate, string? searchText, bool fuzzyMatch)
        {
            // An empty search text imposes no filter.
            if (string.IsNullOrEmpty(searchText))
                return true;

            if (string.IsNullOrEmpty(candidate))
                return false;

            if (!fuzzyMatch)
                return candidate.Contains(searchText, StringComparison.OrdinalIgnoreCase);

            var normalizedSearch = Normalize(searchText);

            // If the search text has no alphanumeric characters, fall back to a plain
            // case-insensitive substring match rather than matching everything.
            if (normalizedSearch.Length == 0)
                return candidate.Contains(searchText, StringComparison.OrdinalIgnoreCase);

            return Normalize(candidate).Contains(normalizedSearch, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns true if any of the supplied candidate values matches the search text.
        /// Used to run the cross-property (Name / AutomationId / ClassName) search in one call.
        /// </summary>
        public static bool IsMatchAny(string? searchText, bool fuzzyMatch, params string?[] candidates)
        {
            if (string.IsNullOrEmpty(searchText))
                return true;

            if (candidates == null)
                return false;

            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrEmpty(candidate) && IsMatch(candidate, searchText, fuzzyMatch))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Normalizes a string for fuzzy matching by lowercasing and dropping any character
        /// that is not a letter or digit.
        /// </summary>
        private static string Normalize(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(char.ToLowerInvariant(c));
            }
            return builder.ToString();
        }
    }
}
