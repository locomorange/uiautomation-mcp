namespace UIAutomationMCP.Models.Results
{
    // Layout and positioning operation results
    
    /// <summary>
    /// Result for element docking operations
    /// </summary>
    public class DockElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string DockPosition { get; set; } = string.Empty;
        public string PreviousDockPosition { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for expand/collapse operations
    /// </summary>
    public class ExpandCollapseElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ExpandCollapseState { get; set; } = string.Empty;
        public string PreviousState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for element move operations
    /// </summary>
    public class MoveElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double PreviousX { get; set; }
        public double PreviousY { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for element resize operations
    /// </summary>
    public class ResizeElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double PreviousWidth { get; set; }
        public double PreviousHeight { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for element rotation operations
    /// </summary>
    public class RotateElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double RotationAngle { get; set; }
        public double PreviousRotationAngle { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for scroll percentage operations
    /// </summary>
    public class SetScrollPercentResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double HorizontalPercent { get; set; }
        public double VerticalPercent { get; set; }
        public double PreviousHorizontalPercent { get; set; }
        public double PreviousVerticalPercent { get; set; }
    }

    /// <summary>
    /// Result for scroll into view operations
    /// </summary>
    public class ScrollElementIntoViewResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? ScrolledElement { get; set; }
        public bool WasAlreadyVisible { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for scroll element operations
    /// </summary>
    public class ScrollElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string ScrollDirection { get; set; } = string.Empty;
        public double Amount { get; set; }
        public double PreviousHorizontalPercent { get; set; }
        public double PreviousVerticalPercent { get; set; }
        public double CurrentHorizontalPercent { get; set; }
        public double CurrentVerticalPercent { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for expand/collapse state changes
    /// </summary>
    public class ExpandCollapseResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string PreviousState { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// General transform operation result
    /// </summary>
    public class TransformActionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string TransformType { get; set; } = string.Empty;
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? Rotation { get; set; }
        public string? PreviousState { get; set; }
        public string? CurrentState { get; set; }
        public string Details { get; set; } = string.Empty;
        public string NewBounds { get; set; } = string.Empty;
        public double? RotationAngle { get; set; }
    }

    /// <summary>
    /// Transform capabilities result
    /// </summary>
    public class TransformResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string TransformType { get; set; } = string.Empty;
        public bool CanMove { get; set; }
        public bool CanResize { get; set; }
        public bool CanRotate { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Transform capabilities information
    /// </summary>
    public class TransformCapabilitiesResult : BaseOperationResult
    {
        public bool CanMove { get; set; }
        public bool CanResize { get; set; }
        public bool CanRotate { get; set; }
        public string TransformType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dock operation result
    /// </summary>
    public class DockResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string DockPosition { get; set; } = string.Empty;
        public string PreviousPosition { get; set; } = string.Empty;
        public List<string> SupportedDockPositions { get; set; } = new List<string>();
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Scroll operation result
    /// </summary>
    public class ScrollResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double HorizontalPercent { get; set; }
        public double VerticalPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Scroll action result
    /// </summary>
    public class ScrollActionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double HorizontalPercent { get; set; }
        public double VerticalPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Scroll information result
    /// </summary>
    public class ScrollInfoResult : BaseOperationResult
    {
        public double HorizontalScrollPercent { get; set; }
        public double VerticalScrollPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
    }
}