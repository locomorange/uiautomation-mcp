using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Core.Options;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Core.Validation;

namespace UIAutomationMCP.Server.Services
{
    public class ElementSearchService : BaseUIAutomationService<SearchServiceMetadata>, IElementSearchService
    {
        private readonly IOptions<UIAutomationOptions> _options;

        public ElementSearchService(
            IProcessManager processManager, 
            ILogger<ElementSearchService> logger,
            IOptions<UIAutomationOptions> options)
            : base(processManager, logger)
        {
            _options = options;
        }

        protected override string GetOperationType() => "search";

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(
            string? windowTitle = null, 
            string? searchText = null, 
            string? controlType = null, 
            long? windowHandle = null, 
            int timeoutSeconds = 60)
        {
            return await FindElementsAsync(windowTitle, searchText, controlType, windowHandle, "descendants", true, 100, false, timeoutSeconds);
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(
            string? windowTitle = null, 
            string? searchText = null, 
            string? controlType = null, 
            long? windowHandle = null, 
            string scope = "descendants", 
            bool validatePatterns = true, 
            int maxResults = 100, 
            bool useCache = true, 
            int timeoutSeconds = 60)
        {
            var request = new FindElementsRequest
            {
                WindowTitle = windowTitle ?? "",
                SearchText = searchText ?? "",
                ControlType = controlType ?? "",
                WindowHandle = windowHandle,
                Scope = scope,
                MaxResults = maxResults,
                UseCache = false, // Always bypass cache for real-time UI state
                ValidatePatterns = validatePatterns,
                UseRegex = false,
                UseWildcard = false
            };

            return await ExecuteServiceOperationAsync<FindElementsRequest, ElementSearchResult>(
                "FindElements",
                request,
                nameof(FindElementsAsync),
                timeoutSeconds,
                ValidateFindElementsRequest
            );
        }

        public async Task<ServerEnhancedResponse<SearchElementsResult>> SearchElementsAsync(
            SearchElementsRequest request, 
            int timeoutSeconds = 30)
        {
            return await ExecuteServiceOperationAsync<SearchElementsRequest, SearchElementsResult>(
                "SearchElements",
                request,
                nameof(SearchElementsAsync),
                timeoutSeconds,
                ValidateSearchElementsRequest
            );
        }

        private static ValidationResult ValidateFindElementsRequest(FindElementsRequest request)
        {
            var errors = new List<string>();

            if (request.MaxResults <= 0)
            {
                errors.Add("MaxResults must be greater than 0");
            }

            if (request.MaxResults > 1000)
            {
                errors.Add("MaxResults cannot exceed 1000");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateSearchElementsRequest(SearchElementsRequest request)
        {
            // Allow even when no search criteria are specified (allow retrieval of all elements)
            return ValidationResult.Success;
        }

        protected override SearchServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);
            metadata.ActionPerformed = "elementsSearched";

            if (data is ElementSearchResult searchResult)
            {
                metadata.ElementsFound = searchResult.Count;
                metadata.TotalResults = searchResult.Items?.Count ?? 0;
            }
            else if (data is SearchElementsResult elementsResult)
            {
                metadata.ElementsFound = elementsResult.Elements?.Length ?? 0;
            }

            return metadata;
        }
    }
}