using System.Collections.Generic;
using Simplify.Web.Controllers.Meta;
using Simplify.Web.Modules.Context;

namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Represents an action filter execution context.
/// </summary>
public interface IActionFilterContext
{
	/// <summary>
	/// Gets the current web context.
	/// </summary>
	IWebContext WebContext { get; }

	/// <summary>
	/// Gets the controller metadata.
	/// </summary>
	IControllerMetadata Controller { get; }

	/// <summary>
	/// Gets the route parameters.
	/// </summary>
	IReadOnlyDictionary<string, object> RouteParameters { get; }

	/// <summary>
	/// Gets additional data shared between filters.
	/// </summary>
	IDictionary<string, object?> Items { get; }

	/// <summary>
	/// Gets or sets the controller response.
	/// </summary>
	ControllerResponse? ControllerResponse { get; set; }
}
