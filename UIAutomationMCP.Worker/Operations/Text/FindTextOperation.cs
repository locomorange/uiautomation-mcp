using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class FindTextOperation : BaseUIAutomationOperation<FindTextRequest, TextSearchResult>
    {
        public FindTextOperation(
            ElementFinderService elementFinderService,
            ILogger<FindTextOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Core.Validation.ValidationResult ValidateRequest(FindTextRequest request)
        {
            if (string.IsNullOrEmpty(request.SearchText))
            {
                return Core.Validation.ValidationResult.Failure("SearchText is required");
            }

            if (string.IsNullOrEmpty(request.AutomationId) && string.IsNullOrEmpty(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            return Core.Validation.ValidationResult.Success;
        }

        protected override Task<TextSearchResult> ExecuteOperationAsync(FindTextRequest request)
        {
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                WindowTitle = request.WindowTitle,
                ProcessId = request.ProcessId,
            }                WindowHandle = request.WindowHandle
            }
            var element = _elementFinderService.FindElement(searchCriteria);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, $"Element with AutomationId '{request.AutomationId}' and Name '{request.Name}' not found");
            }

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
            {
                _logger.LogWarning("Element with AutomationId '{AutomationId}' and Name '{Name}' does not support TextPattern", 
                    request.AutomationId, request.Name);
                throw new UIAutomationElementNotFoundException("Operation", null, "Element does not support TextPattern - text search requires a text control (TextBox, RichTextBox, etc.)");
            }

            try
            {
                var documentRange = textPattern.DocumentRange;
                if (documentRange == null)
                {
                    _logger.LogWarning("DocumentRange is null for element '{AutomationId}'", request.AutomationId);
                    throw new UIAutomationElementNotFoundException("Operation", null, "Cannot access text content - element may not contain text");
                }

                var foundRange = documentRange.FindText(request.SearchText, request.Backward, request.IgnoreCase);
            
                if (foundRange != null)
                {
                    var foundText = foundRange.GetText(-1) ?? "";
                    var boundingRects = foundRange.GetBoundingRectangles();
                    var boundingRect = boundingRects?.Length > 0 ? new BoundingRectangle
                    {
                        X = boundingRects[0].X,
                        Y = boundingRects[0].Y,
                        Width = boundingRects[0].Width,
                        Height = boundingRects[0].Height
                    } : new BoundingRectangle();

                    // Create TextMatch for the found result
                    var textMatch = new TextMatch
                    {
                        StartIndex = 0, // UI Automation doesn't provide exact index
                        EndIndex = foundText.Length,
                        Length = foundText.Length,
                        MatchedText = foundText,
                        BoundingRectangle = boundingRect?.ToString() ?? string.Empty,
                        IsHighlighted = false,
                        IsSelected = false
                    };

                    return Task.FromResult(new TextSearchResult 
                    { 
                        // Primary properties - only set Matches
                        Matches = new List<TextMatch> { textMatch },
                        
                        // Search context
                        SearchText = request.SearchText,
                        CaseSensitive = !request.IgnoreCase,
                        SearchDirection = request.Backward ? "Backward" : "Forward",
                        
                        // Additional context
                        AutomationId = request.AutomationId ?? "",
                        Name = request.Name ?? "",
                        ControlType = request.ControlType ?? "",
                        ProcessId = request.ProcessId ?? 0
                    });
                }
                else
                {
                    return Task.FromResult(new TextSearchResult 
                    { 
                        // Primary properties - empty matches for not found
                        Matches = new List<TextMatch>(),
                        
                        // Search context
                        SearchText = request.SearchText,
                        CaseSensitive = !request.IgnoreCase,
                        SearchDirection = request.Backward ? "Backward" : "Forward",
                        
                        // Additional context
                        AutomationId = request.AutomationId ?? "",
                        Name = request.Name ?? "",
                        ControlType = request.ControlType ?? "",
                        ProcessId = request.ProcessId ?? 0
                    });
                }
            }
            catch (System.Runtime.InteropServices.COMException comEx)
            {
                _logger.LogError(comEx, "COM error during text search operation");
                throw new UIAutomationElementNotFoundException("Operation", null, $"Text search failed due to UI automation error: {comEx.Message}");
            }
            catch (InvalidOperationException invalidOpEx)
            {
                _logger.LogError(invalidOpEx, "Invalid operation during text search");
                throw new UIAutomationElementNotFoundException("Operation", null, $"Text search operation is not valid for this element: {invalidOpEx.Message}");
            }
        }
    }
}