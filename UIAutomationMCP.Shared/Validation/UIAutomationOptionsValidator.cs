using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared.Options;

namespace UIAutomationMCP.Shared.Validation
{
    /// <summary>
    /// UIAutomationOptionsのバリデーター
    /// </summary>
    public class UIAutomationOptionsValidator : IValidateOptions<UIAutomationOptions>
    {
        public ValidateOptionsResult Validate(string? name, UIAutomationOptions options)
        {
            var failures = new List<string>();

            // ElementSearchOptionsの検証
            if (options.ElementSearch != null)
            {
                if (string.IsNullOrEmpty(options.ElementSearch.DefaultScope))
                {
                    failures.Add("ElementSearch.DefaultScope cannot be null or empty");
                }
                else if (!IsValidScope(options.ElementSearch.DefaultScope))
                {
                    failures.Add($"ElementSearch.DefaultScope '{options.ElementSearch.DefaultScope}' is not valid. Valid values: descendants, children, element, parent, ancestors, subtree");
                }

                if (options.ElementSearch.MaxResults <= 0)
                {
                    failures.Add("ElementSearch.MaxResults must be greater than 0");
                }
                else if (options.ElementSearch.MaxResults > 1000)
                {
                    failures.Add("ElementSearch.MaxResults cannot exceed 1000");
                }
            }

            // WindowOperationOptionsの検証
            if (options.WindowOperation != null)
            {
                if (string.IsNullOrEmpty(options.WindowOperation.DefaultAction))
                {
                    failures.Add("WindowOperation.DefaultAction cannot be null or empty");
                }
                else if (!IsValidWindowAction(options.WindowOperation.DefaultAction))
                {
                    failures.Add($"WindowOperation.DefaultAction '{options.WindowOperation.DefaultAction}' is not valid. Valid values: close, minimize, maximize, restore, normal, setfocus");
                }
            }

            // TextOperationOptionsの検証
            if (options.TextOperation != null)
            {
                if (string.IsNullOrEmpty(options.TextOperation.DefaultTraverseUnit))
                {
                    failures.Add("TextOperation.DefaultTraverseUnit cannot be null or empty");
                }
                else if (!IsValidTraverseUnit(options.TextOperation.DefaultTraverseUnit))
                {
                    failures.Add($"TextOperation.DefaultTraverseUnit '{options.TextOperation.DefaultTraverseUnit}' is not valid. Valid values: character, word, line, paragraph, page, document");
                }

                if (options.TextOperation.DefaultTraverseCount <= 0)
                {
                    failures.Add("TextOperation.DefaultTraverseCount must be greater than 0");
                }
                else if (options.TextOperation.DefaultTraverseCount > 1000)
                {
                    failures.Add("TextOperation.DefaultTraverseCount cannot exceed 1000");
                }
            }

            // TransformOptionsの検証
            if (options.Transform != null)
            {
                if (options.Transform.DefaultWidth <= 0)
                {
                    failures.Add("Transform.DefaultWidth must be greater than 0");
                }
                else if (options.Transform.DefaultWidth > 10000)
                {
                    failures.Add("Transform.DefaultWidth cannot exceed 10000");
                }

                if (options.Transform.DefaultHeight <= 0)
                {
                    failures.Add("Transform.DefaultHeight must be greater than 0");
                }
                else if (options.Transform.DefaultHeight > 10000)
                {
                    failures.Add("Transform.DefaultHeight cannot exceed 10000");
                }

                if (options.Transform.DefaultRotationDegrees < -360 || options.Transform.DefaultRotationDegrees > 360)
                {
                    failures.Add("Transform.DefaultRotationDegrees must be between -360 and 360");
                }
            }

            // RangeValueOptionsの検証
            if (options.RangeValue != null)
            {
                if (options.RangeValue.DefaultMinimum > options.RangeValue.DefaultMaximum)
                {
                    failures.Add("RangeValue.DefaultMinimum cannot be greater than DefaultMaximum");
                }

                if (options.RangeValue.DefaultValue < options.RangeValue.DefaultMinimum || 
                    options.RangeValue.DefaultValue > options.RangeValue.DefaultMaximum)
                {
                    failures.Add("RangeValue.DefaultValue must be between DefaultMinimum and DefaultMaximum");
                }

                if (options.RangeValue.DefaultStep <= 0)
                {
                    failures.Add("RangeValue.DefaultStep must be greater than 0");
                }
                else if (options.RangeValue.DefaultStep > 100)
                {
                    failures.Add("RangeValue.DefaultStep cannot exceed 100");
                }
            }

            if (failures.Count > 0)
            {
                return ValidateOptionsResult.Fail(failures);
            }

            return ValidateOptionsResult.Success;
        }

        private static bool IsValidScope(string scope)
        {
            return scope.ToLower() switch
            {
                "descendants" => true,
                "children" => true,
                "element" => true,
                "parent" => true,
                "ancestors" => true,
                "subtree" => true,
                _ => false
            };
        }

        private static bool IsValidWindowAction(string action)
        {
            return action.ToLower() switch
            {
                "close" => true,
                "minimize" => true,
                "maximize" => true,
                "restore" => true,
                "normal" => true,
                "setfocus" => true,
                _ => false
            };
        }

        private static bool IsValidTraverseUnit(string unit)
        {
            return unit.ToLower() switch
            {
                "character" => true,
                "word" => true,
                "line" => true,
                "paragraph" => true,
                "page" => true,
                "document" => true,
                _ => false
            };
        }
    }
}