namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Provides action filter execution result.
/// </summary>
public sealed class ActionFilterResult : IActionFilterResult
{
	private ActionFilterResult(bool isExecutionStopped, ControllerResponse? controllerResponse)
	{
		IsExecutionStopped = isExecutionStopped;
		ControllerResponse = controllerResponse;
	}

	/// <summary>
	/// Gets a value indicating whether controller execution should be stopped.
	/// </summary>
	public bool IsExecutionStopped { get; }

	/// <summary>
	/// Gets the response returned by a filter.
	/// </summary>
	public ControllerResponse? ControllerResponse { get; }

	/// <summary>
	/// Creates a result that continues controller execution.
	/// </summary>
	public static IActionFilterResult Continue() => new ActionFilterResult(false, null);

	/// <summary>
	/// Creates a result that stops controller execution.
	/// </summary>
	/// <param name="response">The response returned by the filter.</param>
	public static IActionFilterResult Stop(ControllerResponse response) => new ActionFilterResult(true, response);
}
