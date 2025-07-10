using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Worker.Contracts
{
    /// <summary>
    /// UI自動化操作を処理するオペレーションのインターフェース
    /// </summary>
    public interface IUIAutomationOperation
    {
        /// <summary>
        /// UI自動化操作を実行する
        /// </summary>
        /// <param name="request">操作リクエスト</param>
        /// <returns>操作結果</returns>
        Task<OperationResult> ExecuteAsync(WorkerRequest request);
    }
}