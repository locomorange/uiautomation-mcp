namespace UIAutomationMCP.Models.Results
{
    // Selection operation results

    /// <summary>
    /// Result for element selection operations
    /// </summary>
    public class SelectElementResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? SelectedElement { get; set; }
        public bool WasAlreadySelected { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for adding to selection operations
    /// </summary>
    public class AddToSelectionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? AddedElement { get; set; }
        public int TotalSelectedCount { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for removing from selection operations
    /// </summary>
    public class RemoveFromSelectionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? RemovedElement { get; set; }
        public int TotalSelectedCount { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for clear selection operations
    /// </summary>
    public class ClearSelectionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public int PreviousSelectedCount { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for select item operations
    /// </summary>
    public class SelectItemResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public ElementInfo? SelectedItem { get; set; }
        public bool WasAlreadySelected { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// General selection action result
    /// </summary>
    public class SelectionActionResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string SelectionType { get; set; } = string.Empty;
        public int CurrentSelectionCount { get; set; }
        public string Details { get; set; } = string.Empty;
        public ElementInfo? SelectedElement { get; set; }
    }

    /// <summary>
    /// Selection information result
    /// </summary>
    public class SelectionInfoResult : BaseOperationResult
    {
        public bool CanSelectMultiple { get; set; }
        public bool IsSelectionRequired { get; set; }
        public List<ElementInfo> SelectedItems { get; set; } = new List<ElementInfo>();
        public int SelectionCount { get; set; }
    }
}