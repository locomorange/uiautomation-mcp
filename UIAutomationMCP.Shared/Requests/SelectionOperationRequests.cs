using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === Selection操作 ===

    public class AddToSelectionRequest : ElementTargetRequest
    {
        public override string Operation => "AddToSelection";
    }


    public class ClearSelectionRequest : ElementTargetRequest
    {
        public override string Operation => "ClearSelection";
    }

    public class GetSelectionContainerRequest : ElementTargetRequest
    {
        public override string Operation => "GetSelectionContainer";
    }

    public class GetSelectionRequest : ElementTargetRequest
    {
        public override string Operation => "GetSelection";
    }

    public class IsSelectedRequest : ElementTargetRequest
    {
        public override string Operation => "IsSelected";
    }


    public class RemoveFromSelectionRequest : ElementTargetRequest
    {
        public override string Operation => "RemoveFromSelection";
    }

    public class SelectElementRequest : ElementTargetRequest
    {
        public override string Operation => "SelectElement";
    }

    public class SelectItemRequest : ElementTargetRequest
    {
        public override string Operation => "SelectItem";
    }

    public class CanSelectMultipleRequest : ElementTargetRequest
    {
        public override string Operation => "CanSelectMultiple";
    }

    public class IsSelectionRequiredRequest : ElementTargetRequest
    {
        public override string Operation => "IsSelectionRequired";
    }
}