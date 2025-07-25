// This interface has been moved to UIAutomationMCP.UIAutomation.Abstractions
// Use UIAutomationMCP.UIAutomation.Abstractions.IUIAutomationOperation instead
// This file is kept for backward compatibility during migration

using UIAutomationMCP.UIAutomation.Abstractions;

namespace UIAutomationMCP.Worker.Contracts
{
    /// <summary>
    /// Legacy interface - use UIAutomationMCP.UIAutomation.Abstractions.IUIAutomationOperation instead
    /// </summary>
    [System.Obsolete("Use UIAutomationMCP.UIAutomation.Abstractions.IUIAutomationOperation instead")]
    public interface IUIAutomationOperation : UIAutomationMCP.UIAutomation.Abstractions.IUIAutomationOperation
    {
    }
}