using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models.Logging;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;

namespace UIAutomationMCP.Server.Infrastructure;

/// <summary>
/// Extension methods for registering UIAutomation services in the DI container.
/// Shared between Program.cs (production) and test infrastructure to ensure
/// service registrations stay in sync.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all core UIAutomation services (log relay, application services,
    /// subprocess-based UI Automation services, and consolidated services).
    /// Does NOT register ProcessManager, MCP Server configuration, or logging providers,
    /// as those are environment-specific.
    /// </summary>
    public static IServiceCollection AddUIAutomationCoreServices(this IServiceCollection services)
    {
        // Log relay service for subprocess logs
        services.AddSingleton<IMcpLogService, LogRelayService>();

        // Application services
        services.AddSingleton<IApplicationLauncher, ApplicationLauncher>();
        services.AddSingleton<IScreenshotService, ScreenshotService>();

        // Subprocess-based UI Automation services
        services.AddSingleton<IElementSearchService, ElementSearchService>();
        services.AddSingleton<ITreeNavigationService, TreeNavigationService>();
        services.AddSingleton<IInvokeService, InvokeService>();
        services.AddSingleton<IValueService, ValueService>();
        services.AddSingleton<IToggleService, ToggleService>();
        services.AddSingleton<ISelectionService, SelectionService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<ITextService, TextService>();
        services.AddSingleton<ILayoutService, LayoutService>();
        services.AddSingleton<IRangeService, RangeService>();

        // Additional subprocess-based UI Automation services
        services.AddSingleton<IGridService, GridService>();
        services.AddSingleton<ITableService, TableService>();
        services.AddSingleton<IMultipleViewService, MultipleViewService>();
        services.AddSingleton<IAccessibilityService, AccessibilityService>();
        services.AddSingleton<ICustomPropertyService, CustomPropertyService>();
        services.AddSingleton<ITransformService, TransformService>();
        services.AddSingleton<IVirtualizedItemService, VirtualizedItemService>();
        services.AddSingleton<IItemContainerService, ItemContainerService>();
        services.AddSingleton<ISynchronizedInputService, SynchronizedInputService>();
        services.AddSingleton<IEventMonitorService, EventMonitorService>();
        services.AddSingleton<IFocusService, FocusService>();

        // Consolidated services
        services.AddSingleton<IInteractionService, InteractionService>();
        services.AddSingleton<IGridTableService, GridTableService>();
        services.AddSingleton<IAdvancedPatternService, AdvancedPatternService>();

        // ControlType service
        services.AddSingleton<IControlTypeService, ControlTypeService>();

        return services;
    }
}
