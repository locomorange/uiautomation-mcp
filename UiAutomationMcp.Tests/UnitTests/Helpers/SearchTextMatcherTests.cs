using UIAutomationMCP.Subprocess.Worker.Helpers;
using Xunit;

namespace UiAutomationMcp.Tests.UnitTests.Helpers
{
    /// <summary>
    /// Unit tests for <see cref="SearchTextMatcher"/> - the cross-property substring/fuzzy
    /// matching used by SearchElements to filter found elements by searchText.
    /// </summary>
    public class SearchTextMatcherTests
    {
        #region Substring (non-fuzzy) matching

        [Theory]
        [InlineData("RichEditBox", "RichEditBox")]   // exact
        [InlineData("RichEditBox", "richeditbox")]   // case-insensitive
        [InlineData("RichEditBox", "EditBox")]       // substring
        [InlineData("RichEditBox", "Edit")]          // substring
        [InlineData("MyRichEditBoxControl", "RichEditBox")]
        public void IsMatch_NonFuzzy_MatchingSubstring_ReturnsTrue(string candidate, string searchText)
        {
            Assert.True(SearchTextMatcher.IsMatch(candidate, searchText, fuzzyMatch: false));
        }

        [Theory]
        [InlineData("WinUI 3 Gallery", "RichEditBox")]
        [InlineData("Minimize", "RichEditBox")]
        [InlineData("Home", "Button")]
        [InlineData("RichEditBox", "Rich Edit Box")] // spaces break the plain substring match
        public void IsMatch_NonFuzzy_NonMatching_ReturnsFalse(string candidate, string searchText)
        {
            Assert.False(SearchTextMatcher.IsMatch(candidate, searchText, fuzzyMatch: false));
        }

        #endregion

        #region Fuzzy (normalized) matching

        [Theory]
        [InlineData("Rich Edit Box", "RichEditBox")]
        [InlineData("rich-edit-box", "RichEditBox")]
        [InlineData("rich_edit_box", "richeditbox")]
        [InlineData("Rich.Edit.Box", "edit box")]
        [InlineData("RichEditBox", "rich edit box")]
        public void IsMatch_Fuzzy_NormalizedMatch_ReturnsTrue(string candidate, string searchText)
        {
            Assert.True(SearchTextMatcher.IsMatch(candidate, searchText, fuzzyMatch: true));
        }

        [Theory]
        [InlineData("WinUI 3 Gallery", "RichEditBox")]
        [InlineData("Home", "RichEditBox")]
        public void IsMatch_Fuzzy_NonMatching_ReturnsFalse(string candidate, string searchText)
        {
            Assert.False(SearchTextMatcher.IsMatch(candidate, searchText, fuzzyMatch: true));
        }

        [Fact]
        public void IsMatch_Fuzzy_SearchTextWithoutAlphanumerics_FallsBackToSubstring()
        {
            // "!!!" normalizes to empty; should not match everything.
            Assert.False(SearchTextMatcher.IsMatch("RichEditBox", "!!!", fuzzyMatch: true));
            Assert.True(SearchTextMatcher.IsMatch("a!!!b", "!!!", fuzzyMatch: true));
        }

        #endregion

        #region Edge cases

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsMatch_EmptyOrNullSearchText_ReturnsTrue(string? searchText)
        {
            // An empty search text imposes no filter.
            Assert.True(SearchTextMatcher.IsMatch("anything", searchText, fuzzyMatch: false));
            Assert.True(SearchTextMatcher.IsMatch("anything", searchText, fuzzyMatch: true));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsMatch_EmptyOrNullCandidate_WithSearchText_ReturnsFalse(string? candidate)
        {
            Assert.False(SearchTextMatcher.IsMatch(candidate, "Button", fuzzyMatch: false));
            Assert.False(SearchTextMatcher.IsMatch(candidate, "Button", fuzzyMatch: true));
        }

        #endregion

        #region Cross-property matching (IsMatchAny)

        [Fact]
        public void IsMatchAny_MatchesOnAnyCandidate()
        {
            // Matches on Name
            Assert.True(SearchTextMatcher.IsMatchAny("Submit", fuzzyMatch: false,
                "Submit Button", "OkButtonId", "Button"));

            // Matches on AutomationId
            Assert.True(SearchTextMatcher.IsMatchAny("OkButtonId", fuzzyMatch: false,
                "Cancel", "OkButtonId", "Button"));

            // Matches on ClassName
            Assert.True(SearchTextMatcher.IsMatchAny("TextBlock", fuzzyMatch: false,
                "Cancel", "CancelId", "TextBlock"));
        }

        [Fact]
        public void IsMatchAny_NoCandidateMatches_ReturnsFalse()
        {
            Assert.False(SearchTextMatcher.IsMatchAny("RichEditBox", fuzzyMatch: false,
                "WinUI 3 Gallery", "HomeItem", "NavigationViewItem"));
        }

        [Fact]
        public void IsMatchAny_IgnoresNullAndEmptyCandidates()
        {
            Assert.True(SearchTextMatcher.IsMatchAny("Button", fuzzyMatch: false,
                null, "", "MyButton"));
            Assert.False(SearchTextMatcher.IsMatchAny("Button", fuzzyMatch: false,
                null, "", null));
        }

        [Fact]
        public void IsMatchAny_EmptySearchText_ReturnsTrue()
        {
            Assert.True(SearchTextMatcher.IsMatchAny("", fuzzyMatch: false, "a", "b"));
        }

        #endregion
    }
}
