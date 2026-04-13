namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Represents an action filter execution result.
/// </summary>
public interface IActionFilterResult
{
	/// <summary>
	/// Gets a value indicating whether controller execution should be stopped.
	/// </summary>
	bool IsExecutionStopped { get; }

	/// <summary>
	/// Gets the response returned by a filter.
	/// </summary>
	ControllerResponse? ControllerResponse { get; }
}
