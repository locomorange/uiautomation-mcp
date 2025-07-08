using System.Windows.Automation;
using System.Windows.Automation.Text;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Worker.Operations
{
    public class TextOperations
    {
        public TextOperations()
        {
        }

        public OperationResult GetText(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            // Try TextPattern first
            if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPattern) && textPattern is TextPattern tp)
            {
                var documentRange = tp.DocumentRange;
                var text = documentRange.GetText(-1);
                
                return new OperationResult 
                { 
                    Success = true, 
                    Data = new { Text = text, HasEmbeddedObjects = false, EmbeddedObjects = new List<object>() }
                };
            }
            // Try ValuePattern for text input controls
            else if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
            {
                return new OperationResult 
                { 
                    Success = true, 
                    Data = new { Text = vp.Current.Value, HasEmbeddedObjects = false, EmbeddedObjects = new List<object>() }
                };
            }
            else
            {
                // Fallback to Name property
                var text = element.Current.Name ?? "";
                return new OperationResult 
                { 
                    Success = true, 
                    Data = new { Text = text, HasEmbeddedObjects = false, EmbeddedObjects = new List<object>() }
                };
            }
        }

        public OperationResult SelectText(string elementId, int startIndex, int length, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                return new OperationResult { Success = false, Error = "Element does not support TextPattern" };

            // Let exceptions flow naturally - no try-catch
            var documentRange = textPattern.DocumentRange;
            var fullText = documentRange.GetText(-1);
            
            if (startIndex < 0 || startIndex >= fullText.Length)
                return new OperationResult { Success = false, Error = "Start index is out of range" };
            
            if (startIndex + length > fullText.Length)
                length = fullText.Length - startIndex;

            var textRange = documentRange.Clone();
            textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, startIndex);
            textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, startIndex);
            textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, length);
            textRange.Select();

            return new OperationResult { Success = true, Data = "Text selected successfully" };
        }

        public OperationResult FindText(string elementId, string searchText, bool backward = false, bool ignoreCase = true, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                return new OperationResult { Success = false, Error = "Element does not support TextPattern" };

            // Let exceptions flow naturally - no try-catch
            var documentRange = textPattern.DocumentRange;
            var foundRange = documentRange.FindText(searchText, backward, ignoreCase);
            
            if (foundRange != null)
            {
                var foundText = foundRange.GetText(-1);
                return new OperationResult 
                { 
                    Success = true, 
                    Data = new { Found = true, Text = foundText, BoundingRectangle = foundRange.GetBoundingRectangles() }
                };
            }
            else
            {
                return new OperationResult { Success = true, Data = new { Found = false } };
            }
        }

        public OperationResult GetTextSelection(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                return new OperationResult { Success = false, Error = "Element does not support TextPattern" };

            // Let exceptions flow naturally - no try-catch
            var selectionRanges = textPattern.GetSelection();
            var selectionInfo = new List<object>();

            foreach (var range in selectionRanges)
            {
                selectionInfo.Add(new
                {
                    Text = range.GetText(-1),
                    BoundingRectangle = range.GetBoundingRectangles()
                });
            }

            return new OperationResult { Success = true, Data = selectionInfo };
        }

        public OperationResult SetText(string elementId, string text, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            // Primary method: Use ValuePattern for text input controls
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
            {
                if (!vp.Current.IsReadOnly)
                {
                    // Let exceptions flow naturally - no try-catch
                    vp.SetValue(text);
                    return new OperationResult { Success = true, Data = "Text set successfully" };
                }
                else
                {
                    return new OperationResult { Success = false, Error = "Element is read-only" };
                }
            }
            
            return new OperationResult { Success = false, Error = "Element does not support text modification" };
        }

        public OperationResult TraverseText(string elementId, string direction, int count = 1, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                return new OperationResult { Success = false, Error = "Element does not support TextPattern" };

            // Let exceptions flow naturally - no try-catch
            var selectionRanges = textPattern.GetSelection();
            var results = new List<object>();

            foreach (var range in selectionRanges)
            {
                var workingRange = range.Clone();
                var (textUnit, moveDirection) = ParseTraversalDirection(direction);
                var actualCount = moveDirection * count;

                var moved = workingRange.Move(textUnit, actualCount);
                workingRange.Select();
                var newText = workingRange.GetText(-1);
                
                results.Add(new
                {
                    MovedUnits = moved,
                    Text = newText,
                    BoundingRectangle = workingRange.GetBoundingRectangles()
                });
            }

            return new OperationResult { Success = true, Data = results };
        }

        public OperationResult GetTextAttributes(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                return new OperationResult { Success = false, Error = "Element does not support TextPattern" };

            // Let exceptions flow naturally - no try-catch
            var selectionRanges = textPattern.GetSelection();
            var attributeResults = new List<object>();

            foreach (var range in selectionRanges)
            {
                var attributes = new Dictionary<string, object>();
                
                // Get common text attributes
                var attributesToCheck = new[]
                {
                    TextPattern.FontNameAttribute,
                    TextPattern.FontSizeAttribute,
                    TextPattern.FontWeightAttribute,
                    TextPattern.ForegroundColorAttribute,
                    TextPattern.BackgroundColorAttribute,
                    TextPattern.IsItalicAttribute
                };

                foreach (var attr in attributesToCheck)
                {
                    // Let exceptions flow naturally - no try-catch
                    var value = range.GetAttributeValue(attr);
                    if (value != null && !value.Equals(TextPattern.MixedAttributeValue))
                    {
                        attributes[attr.ProgrammaticName] = value;
                    }
                    else
                    {
                        attributes[attr.ProgrammaticName] = "NotSupported";
                    }
                }

                attributeResults.Add(new
                {
                    Text = range.GetText(-1),
                    Attributes = attributes,
                    BoundingRectangle = range.GetBoundingRectangles()
                });
            }

            return new OperationResult { Success = true, Data = attributeResults };
        }

        private (TextUnit textUnit, int direction) ParseTraversalDirection(string direction)
        {
            return direction.ToLower() switch
            {
                "character" or "char" => (TextUnit.Character, 1),
                "character-back" or "char-back" => (TextUnit.Character, -1),
                "word" => (TextUnit.Word, 1),
                "word-back" => (TextUnit.Word, -1),
                "line" => (TextUnit.Line, 1),
                "line-back" => (TextUnit.Line, -1),
                "paragraph" => (TextUnit.Paragraph, 1),
                "paragraph-back" => (TextUnit.Paragraph, -1),
                _ => (TextUnit.Character, 1)
            };
        }

        private AutomationElement? FindElementById(string elementId, string windowTitle, int processId)
        {
            var searchRoot = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, elementId);
            return searchRoot.FindFirst(TreeScope.Descendants, condition);
        }

        private AutomationElement? GetSearchRoot(string windowTitle, int processId)
        {
            if (processId > 0)
            {
                var condition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            else if (!string.IsNullOrEmpty(windowTitle))
            {
                var condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            return null;
        }
    }
}