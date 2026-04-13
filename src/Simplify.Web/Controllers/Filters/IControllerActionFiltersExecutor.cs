using System;
using System.Threading.Tasks;

namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Represents a controller action filters executor.
/// </summary>
public interface IControllerActionFiltersExecutor
{
	/// <summary>
	/// Executes controller action filters and controller action itself.
	/// </summary>
	/// <param name="matchedController">The matched controller.</param>
	/// <param name="controllerExecution">The controller execution delegate.</param>
	Task<ControllerResponse?> ExecuteAsync(IMatchedController matchedController, Func<Task<ControllerResponse?>> controllerExecution);
}
