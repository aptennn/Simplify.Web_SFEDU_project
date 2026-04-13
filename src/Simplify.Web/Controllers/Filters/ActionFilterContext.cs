using System.Collections.Generic;
using Simplify.Web.Controllers.Meta;
using Simplify.Web.Modules.Context;

namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Provides action filter execution context.
/// </summary>
/// <param name="webContext">The current web context.</param>
/// <param name="controller">The controller metadata.</param>
/// <param name="routeParameters">The route parameters.</param>
public class ActionFilterContext(IWebContext webContext, IControllerMetadata controller, IReadOnlyDictionary<string, object> routeParameters)
	: IActionFilterContext
{
	/// <summary>
	/// Gets the current web context.
	/// </summary>
	public IWebContext WebContext { get; } = webContext;

	/// <summary>
	/// Gets the controller metadata.
	/// </summary>
	public IControllerMetadata Controller { get; } = controller;

	/// <summary>
	/// Gets the route parameters.
	/// </summary>
	public IReadOnlyDictionary<string, object> RouteParameters { get; } = routeParameters;

	/// <summary>
	/// Gets additional data shared between filters.
	/// </summary>
	public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

	/// <summary>
	/// Gets or sets the controller response.
	/// </summary>
	public ControllerResponse? ControllerResponse { get; set; }
}
