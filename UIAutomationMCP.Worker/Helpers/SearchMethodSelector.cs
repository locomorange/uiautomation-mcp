using System;
using System.Windows.Automation;
using UIAutomationMCP.UIAutomation.Extensions;
using UIAutomationMCP.UIAutomation.Helpers;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// UI Automation検索方法の自動選択ロジック
    /// </summary>
    public static class SearchMethodSelector
    {
        /// <summary>
        /// 検索条件と対象要素に基づいて最適な検索方法を選択
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
                // ルート要素からの検索は危険なのでTreeWalkerを推奨
                if (searchRoot == AutomationElement.RootElement)
                {
                    return SearchMethod.TreeWalker;
                }

                var frameworkId = searchRoot.Current.FrameworkId;
                var controlType = searchRoot.Current.ControlType;

                // フレームワーク別の最適化
                if (IsWpfFramework(frameworkId))
                {
                    // WPFはFindAllが効率的
                    return scope == TreeScope.Children ? SearchMethod.FindAll : SearchMethod.TreeWalker;
                }

                if (IsWin32Framework(frameworkId))
                {
                    // Win32はTreeWalkerが効率的
                    return SearchMethod.TreeWalker;
                }

                // コントロールタイプ別の最適化
                if (IsListContainer(controlType))
                {
                    return ListSearchOptimizer.GetOptimalMethod(searchRoot) == ListSearchMethod.FindAll 
                        ? SearchMethod.FindAll 
                        : SearchMethod.TreeWalker;
                }

                // 検索範囲による最適化
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
                // エラー時はFindAllをデフォルト
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
                recommendation.Warnings.Add("Root要素からの検索は非効率的です。スコープを限定することを推奨します。");
            }

            if (scope == TreeScope.Subtree && complexity == SearchComplexity.Complex)
            {
                recommendation.Warnings.Add("複雑な条件でのSubtree検索は時間がかかる可能性があります。");
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
    /// 検索方法の種類
    /// </summary>
    public enum SearchMethod
    {
        /// <summary>
        /// FindAll方式（一括検索）
        /// </summary>
        FindAll,
        
        /// <summary>
        /// TreeWalker方式（段階的ナビゲーション）
        /// </summary>
        TreeWalker
    }

    /// <summary>
    /// 検索条件の複雑さ
    /// </summary>
    public enum SearchComplexity
    {
        /// <summary>
        /// シンプルな条件（単一プロパティ）
        /// </summary>
        Simple,
        
        /// <summary>
        /// 中程度の複雑さ（2-3個の条件）
        /// </summary>
        Medium,
        
        /// <summary>
        /// 複雑な条件（4個以上の条件、複雑なOR/NOT）
        /// </summary>
        Complex
    }

    /// <summary>
    /// 検索推奨設定
    /// </summary>
    public class SearchRecommendation
    {
        public SearchMethod Method { get; set; }
        public SearchComplexity Complexity { get; set; }
        public bool UseCache { get; set; }
        public int TimeoutSeconds { get; set; }
        public int MaxResults { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }
}