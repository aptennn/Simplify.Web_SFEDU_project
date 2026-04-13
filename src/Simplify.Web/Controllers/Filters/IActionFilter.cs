using System.Threading.Tasks;

namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Represents an action filter.
/// </summary>
public interface IActionFilter
{
	/// <summary>
	/// Executes before a controller action.
	/// </summary>
	/// <param name="context">The filter execution context.</param>
	Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context);

	/// <summary>
	/// Executes after a controller action.
	/// </summary>
	/// <param name="context">The filter execution context.</param>
	Task OnActionExecutedAsync(IActionFilterContext context);
}
