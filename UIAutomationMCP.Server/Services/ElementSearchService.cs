using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Validation;

namespace UIAutomationMCP.Server.Services
{
    public class ElementSearchService : BaseUIAutomationService, IElementSearchService
    {
        private readonly IOptions<UIAutomationOptions> _options;

        public ElementSearchService(
            IOperationExecutor executor, 
            ILogger<ElementSearchService> logger,
            IOptions<UIAutomationOptions> options)
            : base(executor, logger)
        {
            _options = options;
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(
            string? windowTitle = null, 
            string? searchText = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 60)
        {
            return await FindElementsAsync(windowTitle, searchText, controlType, processId, "descendants", true, 100, true, timeoutSeconds);
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(
            string? windowTitle = null, 
            string? searchText = null, 
            string? controlType = null, 
            int? processId = null, 
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
                ProcessId = processId ?? 0,
                Scope = scope,
                MaxResults = maxResults,
                UseCache = useCache,
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
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && 
                string.IsNullOrWhiteSpace(request.Name) && 
                string.IsNullOrWhiteSpace(request.ControlType))
            {
                errors.Add("At least one search criteria (AutomationId, Name, or ControlType) must be provided");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        protected override Dictionary<string, object> CreateSuccessAdditionalInfo<TResult>(TResult data, IServiceContext context)
        {
            var baseInfo = base.CreateSuccessAdditionalInfo(data, context);
            baseInfo["operationType"] = "search";

            if (data is ElementSearchResult searchResult)
            {
                baseInfo["elementsFound"] = searchResult.Count;
                baseInfo["totalResults"] = searchResult.Items?.Count ?? 0;
                baseInfo["actionPerformed"] = "elementsSearched";
            }
            else if (data is SearchElementsResult elementsResult)
            {
                baseInfo["elementsFound"] = elementsResult.Elements?.Length ?? 0;
                baseInfo["actionPerformed"] = "elementsSearched";
            }

            return baseInfo;
        }

        protected override Dictionary<string, object> CreateRequestParameters(IServiceContext context)
        {
            var baseParams = base.CreateRequestParameters(context);
            baseParams["operation"] = context.MethodName.Replace("Async", "");
            return baseParams;
        }
    }
}