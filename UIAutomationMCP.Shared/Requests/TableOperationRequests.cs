using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === Table操作 ===

    public class GetColumnHeaderItemsRequest : ElementTargetRequest
    {
        public override string Operation => "GetColumnHeaderItems";
    }

    public class GetColumnHeadersRequest : ElementTargetRequest
    {
        public override string Operation => "GetColumnHeaders";
    }

    public class GetRowHeaderItemsRequest : ElementTargetRequest
    {
        public override string Operation => "GetRowHeaderItems";
    }

    public class GetRowHeadersRequest : ElementTargetRequest
    {
        public override string Operation => "GetRowHeaders";
    }


}