using System;
using System.Windows.Automation;
using UIAutomationMCP.Common.Extensions;
using UIAutomationMCP.Common.Helpers;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// Automatic selection logic for UI Automation search methods
    /// </summary>
    public static class SearchMethodSelector
    {
        /// <summary>
        /// Select optimal search method based on search criteria and target elements
        /// </summary>
        public static SearchMethod SelectOptimalMethod(
            AutomationElement searchRoot,
            Condition condition,
            TreeScope scope,
            int expectedResultCount = -1)
        {
            if (searchRoot == null || condition == null)
                return SearchMethod.FindAll;

            try
            {
                // Searching from root element is risky, so TreeWalker is recommended
                if (searchRoot == AutomationElement.RootElement)
                {
                    return SearchMethod.TreeWalker;
                }

                var frameworkId = searchRoot.Current.FrameworkId;
                var controlType = searchRoot.Current.ControlType;

                // Framework-specific optimization
                if (IsWpfFramework(frameworkId))
                {
                    // FindAll is efficient for WPF
                    return scope == TreeScope.Children ? SearchMethod.FindAll : SearchMethod.TreeWalker;
                }

                if (IsWin32Framework(frameworkId))
                {
                    // TreeWalker is efficient for Win32
                    return SearchMethod.TreeWalker;
                }

                // Optimization by control type
                if (IsListContainer(controlType))
                {
                    return ListSearchOptimizer.GetOptimalMethod(searchRoot) == ListSearchMethod.FindAll 
                        ? SearchMethod.FindAll 
                        : SearchMethod.TreeWalker;
                }

                // Optimization based on search scope
                return scope switch
                {
                    TreeScope.Children => SearchMethod.FindAll,
                    TreeScope.Descendants => expectedResultCount <= 10 ? SearchMethod.FindAll : SearchMethod.TreeWalker,
                    TreeScope.Subtree => SearchMethod.TreeWalker,
                    _ => SearchMethod.FindAll
                };
            }
            catch
            {
                // Default to FindAll in case of error
                return SearchMethod.FindAll;
            }
        }

        /// <summary>
        /// 検索条件の複雑さを評価
        /// </summary>
        public static SearchComplexity EvaluateComplexity(Condition condition)
        {
            if (condition == null)
                return SearchComplexity.Simple;

            return condition switch
            {
                PropertyCondition => SearchComplexity.Simple,
                AndCondition and => and.GetConditions().Length <= 3 ? SearchComplexity.Medium : SearchComplexity.Complex,
                OrCondition or => or.GetConditions().Length <= 2 ? SearchComplexity.Medium : SearchComplexity.Complex,
                NotCondition => SearchComplexity.Medium,
                _ => SearchComplexity.Simple
            };
        }

        /// <summary>
        /// パフォーマンス予測に基づく推奨設定
        /// </summary>
        public static SearchRecommendation GetRecommendation(
            AutomationElement searchRoot,
            Condition condition,
            TreeScope scope)
        {
            var method = SelectOptimalMethod(searchRoot, condition, scope);
            var complexity = EvaluateComplexity(condition);
            
            var recommendation = new SearchRecommendation
            {
                Method = method,
                Complexity = complexity,
                UseCache = complexity != SearchComplexity.Simple,
                TimeoutSeconds = GetRecommendedTimeout(complexity, scope),
                MaxResults = GetRecommendedMaxResults(scope, complexity)
            };

            // 特別な警告
            if (searchRoot == AutomationElement.RootElement)
            {
                recommendation.Warnings.Add("Searching from the Root element is inefficient. It is recommended to limit the search scope.");
            }

            if (scope == TreeScope.Subtree && complexity == SearchComplexity.Complex)
            {
                recommendation.Warnings.Add("Searching with complex conditions and Subtree scope may take a long time.");
            }

            return recommendation;
        }

        private static bool IsWpfFramework(string frameworkId)
        {
            return !string.IsNullOrEmpty(frameworkId) &&
                   (frameworkId.Equals("WPF", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("WinUI", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("UWP", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsWin32Framework(string frameworkId)
        {
            return !string.IsNullOrEmpty(frameworkId) &&
                   (frameworkId.Equals("Win32", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("WinForm", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("Windows Forms", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsListContainer(ControlType controlType)
        {
            return controlType == ControlType.List ||
                   controlType == ControlType.DataGrid ||
                   controlType == ControlType.Tree ||
                   controlType == ControlType.ComboBox;
        }

        private static int GetRecommendedTimeout(SearchComplexity complexity, TreeScope scope)
        {
            return complexity switch
            {
                SearchComplexity.Simple => scope == TreeScope.Children ? 3 : 5,
                SearchComplexity.Medium => scope == TreeScope.Subtree ? 10 : 8,
                SearchComplexity.Complex => 15,
                _ => 8
            };
        }

        private static int GetRecommendedMaxResults(TreeScope scope, SearchComplexity complexity)
        {
            return scope switch
            {
                TreeScope.Children => 50,
                TreeScope.Descendants => complexity == SearchComplexity.Complex ? 20 : 100,
                TreeScope.Subtree => 10,
                _ => 100
            };
        }
    }

    /// <summary>
    /// Types of search methods
    /// </summary>
    public enum SearchMethod
    {
        /// <summary>
        /// FindAll method (batch search)
        /// </summary>
        FindAll,
        
        /// <summary>
        /// TreeWalker method (step-by-step navigation)
        /// </summary>
        TreeWalker
    }

    /// <summary>
    /// Complexity of the search condition
    /// </summary>
    public enum SearchComplexity
    {
        /// <summary>
        /// Simple condition (single property)
        /// </summary>
        Simple,
        
        /// <summary>
        /// Medium complexity (2-3 conditions)
        /// </summary>
        Medium,
        
        /// <summary>
        /// Complex condition (4 or more conditions, complex OR/NOT)
        /// </summary>
        Complex
    }

    /// <summary>
    /// Search recommendation settings
    /// </summary>
    public class SearchRecommendation
    {
        /// <summary>
        /// Recommended search method
        /// </summary>
        public SearchMethod Method { get; set; }

        /// <summary>
        /// Complexity of the search condition
        /// </summary>
        public SearchComplexity Complexity { get; set; }

        /// <summary>
        /// Whether to use cache for the search
        /// </summary>
        public bool UseCache { get; set; }

        /// <summary>
        /// Recommended timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Maximum number of results to return
        /// </summary>
        public int MaxResults { get; set; }

        /// <summary>
        /// List of warnings or cautions for the search
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }
}