using System.Diagnostics;
using System.Threading.Tasks;
using Simplify.Web.Attributes;

namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Provides a logging action filter.
/// </summary>
[ActionFilter]
public class LoggingActionFilter : IActionFilter
{
	private const string StopwatchItemKey = "simplify.web.actionFilter.logging.stopwatch";

	/// <summary>
	/// Executes before a controller action.
	/// </summary>
	/// <param name="context">The filter execution context.</param>
	public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
	{
		context.Items[StopwatchItemKey] = Stopwatch.StartNew();

		Trace.TraceInformation(
			$"[Simplify.Web] Executing controller '{context.Controller.ControllerType.FullName}' for route '{context.WebContext.Route}'.");

		return Task.FromResult(ActionFilterResult.Continue());
	}

	/// <summary>
	/// Executes after a controller action.
	/// </summary>
	/// <param name="context">The filter execution context.</param>
	public Task OnActionExecutedAsync(IActionFilterContext context)
	{
		var elapsedMilliseconds = 0L;

		if (context.Items.TryGetValue(StopwatchItemKey, out var watchObj) && watchObj is Stopwatch watch)
		{
			watch.Stop();
			elapsedMilliseconds = watch.ElapsedMilliseconds;
		}

		Trace.TraceInformation(
			$"[Simplify.Web] Executed controller '{context.Controller.ControllerType.FullName}' in {elapsedMilliseconds}ms.");

		return Task.CompletedTask;
	}
}
