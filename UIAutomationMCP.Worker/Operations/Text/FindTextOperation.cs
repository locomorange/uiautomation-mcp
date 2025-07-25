using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class FindTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<FindTextOperation> _logger;

        public FindTextOperation(
            ElementFinderService elementFinderService,
            ILogger<FindTextOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<FindTextRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name, 
                    controlType: typedRequest.ControlType, 
                    windowTitle: typedRequest.WindowTitle, 
                    processId: typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Element with AutomationId '{typedRequest.AutomationId}' and Name '{typedRequest.Name}' not found",
                        Data = new TextSearchResult 
                        { 
                            Matches = new List<TextMatch>(),
                            SearchText = typedRequest.SearchText
                        }
                    });
                }

                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                {
                    _logger.LogWarning("Element with AutomationId '{AutomationId}' and Name '{Name}' does not support TextPattern", 
                        typedRequest.AutomationId, typedRequest.Name);
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support TextPattern - text search requires a text control (TextBox, RichTextBox, etc.)",
                        Data = new TextSearchResult 
                        { 
                            Matches = new List<TextMatch>(),
                            SearchText = typedRequest.SearchText
                        }
                    });
                }

                try
                {
                    var documentRange = textPattern.DocumentRange;
                    if (documentRange == null)
                    {
                        _logger.LogWarning("DocumentRange is null for element '{AutomationId}'", typedRequest.AutomationId);
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = false, 
                            Error = "Cannot access text content - element may not contain text",
                            Data = new TextSearchResult { 
                                Matches = new List<TextMatch>(),
                                SearchText = typedRequest.SearchText 
                            }
                        });
                    }

                    var foundRange = documentRange.FindText(typedRequest.SearchText, typedRequest.Backward, typedRequest.IgnoreCase);
                
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
                            BoundingRectangle = boundingRect,
                            IsHighlighted = false,
                            IsSelected = false
                        };

                        return Task.FromResult(new OperationResult 
                        { 
                            Success = true, 
                            Data = new TextSearchResult 
                            { 
                                // Primary properties - only set Matches
                                Matches = new List<TextMatch> { textMatch },
                                
                                // Search context
                                SearchText = typedRequest.SearchText,
                                CaseSensitive = !typedRequest.IgnoreCase,
                                SearchDirection = typedRequest.Backward ? "Backward" : "Forward",
                                
                                // Additional context
                                AutomationId = typedRequest.AutomationId ?? "",
                                Name = typedRequest.Name ?? "",
                                ControlType = typedRequest.ControlType ?? "",
                                ProcessId = typedRequest.ProcessId ?? 0
                            }
                        });
                    }
                    else
                    {
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = true, 
                            Data = new TextSearchResult 
                            { 
                                // Primary properties - empty matches for not found
                                Matches = new List<TextMatch>(),
                                
                                // Search context
                                SearchText = typedRequest.SearchText,
                                CaseSensitive = !typedRequest.IgnoreCase,
                                SearchDirection = typedRequest.Backward ? "Backward" : "Forward",
                                
                                // Additional context
                                AutomationId = typedRequest.AutomationId ?? "",
                                Name = typedRequest.Name ?? "",
                                ControlType = typedRequest.ControlType ?? "",
                                ProcessId = typedRequest.ProcessId ?? 0
                            }
                        });
                    }
                }
                catch (System.Runtime.InteropServices.COMException comEx)
                {
                    _logger.LogError(comEx, "COM error during text search operation");
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Text search failed due to UI automation error: {comEx.Message}",
                        Data = new TextSearchResult { 
                            Matches = new List<TextMatch>(),
                            SearchText = typedRequest.SearchText 
                        }
                    });
                }
                catch (InvalidOperationException invalidOpEx)
                {
                    _logger.LogError(invalidOpEx, "Invalid operation during text search");
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Text search operation is not valid for this element: {invalidOpEx.Message}",
                        Data = new TextSearchResult { 
                            Matches = new List<TextMatch>(),
                            SearchText = typedRequest.SearchText 
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FindText operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to find text: {ex.Message}",
                    Data = new TextSearchResult { 
                        Matches = new List<TextMatch>(),
                        SearchText = "" 
                    }
                });
            }
        }
    }
}