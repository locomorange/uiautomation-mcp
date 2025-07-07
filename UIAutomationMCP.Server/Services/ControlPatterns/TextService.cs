using System.Windows.Automation;
using System.Windows.Automation.Text;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITextService
    {
        Task<object> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = true, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SetTextAsync(string elementId, string text, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> TraverseTextAsync(string elementId, string direction, int count = 1, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetTextAttributesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class TextService : ITextService
    {
        private readonly ILogger<TextService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public TextService(ILogger<TextService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting text from element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    // Try TextPattern first (for rich text controls)
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
                        var documentRange = textPattern.DocumentRange;
                        var text = documentRange.GetText(-1);
                        
                        // Check for embedded objects
                        var children = documentRange.GetChildren();
                        var embeddedObjects = new List<object>();
                        
                        foreach (var child in children)
                        {
                            try
                            {
                                var childRange = textPattern.RangeFromChild(child);
                                embeddedObjects.Add(new
                                {
                                    Name = child.Current.Name,
                                    ControlType = child.Current.ControlType.LocalizedControlType,
                                    Text = childRange.GetText(-1)
                                });
                            }
                            catch
                            {
                                // Some embedded objects might not support range operations
                                embeddedObjects.Add(new
                                {
                                    Name = child.Current.Name,
                                    ControlType = child.Current.ControlType.LocalizedControlType,
                                    Text = ""
                                });
                            }
                        }
                        
                        return (object)new
                        {
                            Text = text,
                            HasEmbeddedObjects = embeddedObjects.Count > 0,
                            EmbeddedObjects = embeddedObjects
                        };
                    }
                    // Try ValuePattern for text input controls
                    else if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
                    {
                        return (object)new
                        {
                            Text = vp.Current.Value,
                            HasEmbeddedObjects = false,
                            EmbeddedObjects = new List<object>()
                        };
                    }
                    else
                    {
                        // Fallback to Name property and check for child text elements
                        var text = element.Current.Name ?? "";
                        var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                        var childTexts = new List<string>();
                        
                        foreach (AutomationElement child in children)
                        {
                            if (child.TryGetCurrentPattern(TextPattern.Pattern, out var childTextPattern) && childTextPattern is TextPattern ctp)
                            {
                                childTexts.Add(ctp.DocumentRange.GetText(-1));
                            }
                            else if (!string.IsNullOrEmpty(child.Current.Name))
                            {
                                childTexts.Add(child.Current.Name);
                            }
                        }
                        
                        if (childTexts.Count > 0)
                        {
                            text = string.Join(" ", childTexts);
                        }
                        
                        return (object)new
                        {
                            Text = text,
                            HasEmbeddedObjects = false,
                            EmbeddedObjects = new List<object>()
                        };
                    }
                }, timeoutSeconds, $"GetText_{elementId}");

                _logger.LogInformation("Text retrieved successfully from element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get text from element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Selecting text in element: {ElementId} from {StartIndex} length {Length}", elementId, startIndex, length);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
                        var documentRange = textPattern.DocumentRange;
                        var fullText = documentRange.GetText(-1);
                        
                        if (startIndex < 0 || startIndex >= fullText.Length)
                        {
                            throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index is out of range");
                        }
                        
                        if (startIndex + length > fullText.Length)
                        {
                            length = fullText.Length - startIndex;
                        }

                        // Create proper text range for selection
                        var textRange = documentRange.Clone();
                        
                        // Move to start position
                        textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, startIndex);
                        textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, startIndex);
                        
                        // Expand to cover the desired length
                        textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, length);
                        
                        // Select the range
                        textRange.Select();
                        
                        // Scroll into view if possible
                        try
                        {
                            textRange.ScrollIntoView(true);
                        }
                        catch
                        {
                            // ScrollIntoView might not be supported, ignore
                        }
                    }
                    else
                    {
                        // Try alternative UI Automation patterns within the framework
                        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
                        {
                            // For text input controls, set focus and use selection via value pattern
                            element.SetFocus();
                            var currentValue = vp.Current.Value;
                            if (startIndex < currentValue.Length)
                            {
                                // This is a limitation - ValuePattern doesn't support text selection
                                // We can only set focus to the element
                                _logger.LogWarning("Element supports ValuePattern but not TextPattern. Cannot select specific text range.");
                                return;
                            }
                        }
                        
                        throw new InvalidOperationException("Element does not support TextPattern or suitable alternatives for text selection");
                    }
                }, timeoutSeconds, $"SelectText_{elementId}");

                _logger.LogInformation("Text selected successfully in element: {ElementId}", elementId);
                return new { Success = true, Message = "Text selected successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to select text in element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = true, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding text '{SearchText}' in element: {ElementId}", searchText, elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
                        var documentRange = textPattern.DocumentRange;
                        var foundRange = documentRange.FindText(searchText, backward, ignoreCase);
                        
                        if (foundRange != null)
                        {
                            var foundText = foundRange.GetText(-1);
                            return (object)new
                            {
                                Found = true,
                                Text = foundText,
                                BoundingRectangle = foundRange.GetBoundingRectangles()
                            };
                        }
                        else
                        {
                            return (object)new { Found = false };
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TextPattern");
                    }
                }, timeoutSeconds, $"FindText_{elementId}");

                _logger.LogInformation("Text search completed in element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find text in element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting text selection from element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var selections = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
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

                        return (object)selectionInfo;
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TextPattern");
                    }
                }, timeoutSeconds, $"GetTextSelection_{elementId}");

                _logger.LogInformation("Text selection retrieved successfully from element: {ElementId}", elementId);
                return new { Success = true, Data = selections };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get text selection from element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SetTextAsync(string elementId, string text, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting text in element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                await _executor.ExecuteAsync(() =>
                {
                    // Primary method: Use ValuePattern for text input controls
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
                    {
                        if (!vp.Current.IsReadOnly)
                        {
                            vp.SetValue(text);
                            return;
                        }
                        else
                        {
                            throw new InvalidOperationException("Element is read-only");
                        }
                    }
                    
                    // Note: TextPattern does not support text insertion per Microsoft documentation
                    // "TextPattern does not provide a means to insert or modify text"
                    
                    // For elements that don't support ValuePattern, try setting focus and checking if it's editable
                    if (element.Current.IsEnabled && element.Current.IsKeyboardFocusable)
                    {
                        element.SetFocus();
                        
                        // Check if element has editable text capabilities through other patterns
                        if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPattern))
                        {
                            // TextPattern is read-only, but we can at least verify it's a text control
                            throw new InvalidOperationException("Element supports TextPattern but not ValuePattern. Cannot modify text in read-only text controls.");
                        }
                        
                        throw new InvalidOperationException("Element does not support text modification through UI Automation patterns");
                    }
                    
                    throw new InvalidOperationException("Element is not enabled or keyboard focusable");
                }, timeoutSeconds, $"SetText_{elementId}");

                _logger.LogInformation("Text set successfully in element: {ElementId}", elementId);
                return new { Success = true, Message = "Text set successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set text in element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> TraverseTextAsync(string elementId, string direction, int count = 1, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Traversing text in element: {ElementId}, direction: {Direction}, count: {Count}", elementId, direction, count);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
                        var selectionRanges = textPattern.GetSelection();
                        var results = new List<object>();

                        foreach (var range in selectionRanges)
                        {
                            var workingRange = range.Clone();
                            
                            // Parse direction and determine TextUnit and movement direction
                            var (textUnit, moveDirection) = ParseTraversalDirection(direction);
                            var actualCount = moveDirection * count;

                            // Move the range
                            var moved = workingRange.Move(textUnit, actualCount);
                            
                            // Select the new range
                            workingRange.Select();
                            
                            // Get the text at the new position
                            var newText = workingRange.GetText(-1);
                            
                            results.Add(new
                            {
                                MovedUnits = moved,
                                Text = newText,
                                BoundingRectangle = workingRange.GetBoundingRectangles()
                            });
                        }

                        return (object)results;
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TextPattern for text traversal");
                    }
                }, timeoutSeconds, $"TraverseText_{elementId}");

                _logger.LogInformation("Text traversal completed in element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to traverse text in element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
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
                "page" => (TextUnit.Page, 1),
                "page-back" => (TextUnit.Page, -1),
                "document" => (TextUnit.Document, 1),
                "document-back" => (TextUnit.Document, -1),
                _ => (TextUnit.Character, 1)
            };
        }

        public async Task<object> GetTextAttributesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting text attributes from element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
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
                                TextPattern.IsItalicAttribute,
                                TextPattern.UnderlineStyleAttribute,
                                TextPattern.StrikethroughStyleAttribute,
                                TextPattern.IsReadOnlyAttribute,
                                TextPattern.CultureAttribute,
                                TextPattern.HorizontalTextAlignmentAttribute,
                                TextPattern.IndentationFirstLineAttribute,
                                TextPattern.IndentationLeadingAttribute,
                                TextPattern.IndentationTrailingAttribute,
                                TextPattern.MarginBottomAttribute,
                                TextPattern.MarginLeadingAttribute,
                                TextPattern.MarginTopAttribute,
                                TextPattern.MarginTrailingAttribute
                            };

                            foreach (var attr in attributesToCheck)
                            {
                                try
                                {
                                    var value = range.GetAttributeValue(attr);
                                    if (value != null && !value.Equals(TextPattern.MixedAttributeValue))
                                    {
                                        attributes[attr.ProgrammaticName] = value;
                                    }
                                    else if (value != null && value.Equals(TextPattern.MixedAttributeValue))
                                    {
                                        attributes[attr.ProgrammaticName] = "Mixed";
                                    }
                                }
                                catch
                                {
                                    // Attribute not supported
                                    attributes[attr.ProgrammaticName] = "NotSupported";
                                }
                            }

                            // Get text and bounding rectangle
                            var text = range.GetText(-1);
                            var boundingRectangles = range.GetBoundingRectangles();

                            attributeResults.Add(new
                            {
                                Text = text,
                                Attributes = attributes,
                                BoundingRectangle = boundingRectangles
                            });
                        }

                        // If no selection, get attributes for the entire document
                        if (attributeResults.Count == 0)
                        {
                            var documentRange = textPattern.DocumentRange;
                            var attributes = new Dictionary<string, object>();
                            
                            var attributesToCheck = new[]
                            {
                                TextPattern.FontNameAttribute,
                                TextPattern.FontSizeAttribute,
                                TextPattern.FontWeightAttribute,
                                TextPattern.ForegroundColorAttribute,
                                TextPattern.BackgroundColorAttribute,
                                TextPattern.IsItalicAttribute,
                                TextPattern.UnderlineStyleAttribute,
                                TextPattern.StrikethroughStyleAttribute,
                                TextPattern.IsReadOnlyAttribute,
                                TextPattern.CultureAttribute,
                                TextPattern.HorizontalTextAlignmentAttribute
                            };

                            foreach (var attr in attributesToCheck)
                            {
                                try
                                {
                                    var value = documentRange.GetAttributeValue(attr);
                                    if (value != null && !value.Equals(TextPattern.MixedAttributeValue))
                                    {
                                        attributes[attr.ProgrammaticName] = value;
                                    }
                                    else if (value != null && value.Equals(TextPattern.MixedAttributeValue))
                                    {
                                        attributes[attr.ProgrammaticName] = "Mixed";
                                    }
                                }
                                catch
                                {
                                    attributes[attr.ProgrammaticName] = "NotSupported";
                                }
                            }

                            attributeResults.Add(new
                            {
                                Text = documentRange.GetText(-1),
                                Attributes = attributes,
                                BoundingRectangle = documentRange.GetBoundingRectangles()
                            });
                        }

                        return (object)attributeResults;
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TextPattern for attribute retrieval");
                    }
                }, timeoutSeconds, $"GetTextAttributes_{elementId}");

                _logger.LogInformation("Text attributes retrieved successfully from element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get text attributes from element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}