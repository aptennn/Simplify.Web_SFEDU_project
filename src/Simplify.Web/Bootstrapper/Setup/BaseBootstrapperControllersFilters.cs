using Simplify.Web.Controllers.Filters;

namespace Simplify.Web.Bootstrapper.Setup;

/// <summary>
/// Provides the bootstrapper action filters registration API.
/// </summary>
public partial class BaseBootstrapper
{
	/// <summary>
	/// Registers global action filters.
	/// </summary>
	public virtual void RegisterActionFilters()
	{
	}

	/// <summary>
	/// Registers a global action filter.
	/// </summary>
	/// <typeparam name="TFilter">The action filter type.</typeparam>
	/// <param name="order">The filter execution order.</param>
	protected void RegisterGlobalFilter<TFilter>(int order = 0)
		where TFilter : class, IActionFilter =>
		_globalActionFilters.Add(new ActionFilterRegistration(typeof(TFilter), order, true));

	/// <summary>
	/// Registers a global action filter with options.
	/// </summary>
	/// <typeparam name="TFilter">The action filter type.</typeparam>
	/// <param name="options">The filter options.</param>
	/// <param name="order">The filter execution order.</param>
	protected void RegisterGlobalFilter<TFilter>(object options, int order = 0)
		where TFilter : class, IActionFilter =>
		_globalActionFilters.Add(new ActionFilterRegistration(typeof(TFilter), order, true, options));
}
