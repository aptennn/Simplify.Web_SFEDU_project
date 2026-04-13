using System;

namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Represents an action filter registration.
/// </summary>
/// <param name="filterType">The filter type.</param>
/// <param name="order">The filter execution order.</param>
/// <param name="isGlobal">Indicates that the filter is global.</param>
/// <param name="options">The filter options.</param>
public class ActionFilterRegistration(Type filterType, int order = 0, bool isGlobal = false, object? options = null)
{
	/// <summary>
	/// Gets the filter type.
	/// </summary>
	public Type FilterType { get; } = filterType;

	/// <summary>
	/// Gets the filter execution order.
	/// </summary>
	public int Order { get; } = order;

	/// <summary>
	/// Gets a value indicating whether the filter is global.
	/// </summary>
	public bool IsGlobal { get; } = isGlobal;

	/// <summary>
	/// Gets the filter options.
	/// </summary>
	public object? Options { get; } = options;
}
