using System;

namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Provides options for <see cref="CacheActionFilter" />.
/// </summary>
public class CacheFilterOptions
{
	/// <summary>
	/// Gets or sets cache duration.
	/// </summary>
	public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);
}
