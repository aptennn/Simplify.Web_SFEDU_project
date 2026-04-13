using System;
using Simplify.Web.Controllers.Filters;

namespace Simplify.Web.Attributes;

/// <summary>
/// Applies an action filter to a controller.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ApplyFilterAttribute"/> class.
/// </remarks>
/// <param name="filterType">The action filter type.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ApplyFilterAttribute(Type filterType) : Attribute
{
	/// <summary>
	/// Gets the filter type.
	/// </summary>
	public Type FilterType { get; } = typeof(IActionFilter).IsAssignableFrom(filterType)
		? filterType
		: throw new ArgumentException($"Filter type should implement interface '{nameof(IActionFilter)}'.", nameof(filterType));

	/// <summary>
	/// Gets or sets the filter execution order.
	/// </summary>
	public int Order { get; set; }
}
