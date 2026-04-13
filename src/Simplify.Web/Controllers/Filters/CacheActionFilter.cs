using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Simplify.Web.Attributes;

namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Provides a controller response cache action filter.
/// </summary>
[ActionFilter]
public class CacheActionFilter : IActionFilter
{
	private const string CacheKeyItemKey = "simplify.web.actionFilter.cache.key";

	private static readonly ConcurrentDictionary<string, CacheItem> Cache = new();

	private readonly CacheFilterOptions _options;

	/// <summary>
	/// Initializes a new instance of the <see cref="CacheActionFilter"/> class.
	/// </summary>
	public CacheActionFilter() : this(new CacheFilterOptions())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CacheActionFilter"/> class.
	/// </summary>
	/// <param name="options">The cache options.</param>
	public CacheActionFilter(CacheFilterOptions options) => _options = options;

	/// <summary>
	/// Executes before a controller action.
	/// </summary>
	/// <param name="context">The filter execution context.</param>
	public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
	{
		if (!string.Equals(context.WebContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
			return Task.FromResult(ActionFilterResult.Continue());

		var key = BuildCacheKey(context);

		context.Items[CacheKeyItemKey] = key;

		if (!Cache.TryGetValue(key, out var cachedItem))
			return Task.FromResult(ActionFilterResult.Continue());

		if (cachedItem.ExpirationTimeUtc <= DateTime.UtcNow)
		{
			Cache.TryRemove(key, out _);
			return Task.FromResult(ActionFilterResult.Continue());
		}

		return Task.FromResult(ActionFilterResult.Stop(cachedItem.Response));
	}

	/// <summary>
	/// Executes after a controller action.
	/// </summary>
	/// <param name="context">The filter execution context.</param>
	public Task OnActionExecutedAsync(IActionFilterContext context)
	{
		if (!context.Items.TryGetValue(CacheKeyItemKey, out var cacheKeyObj) || cacheKeyObj is not string cacheKey)
			return Task.CompletedTask;

		if (context.ControllerResponse == null)
			return Task.CompletedTask;

		Cache[cacheKey] = new CacheItem(
			context.ControllerResponse,
			DateTime.UtcNow.Add(_options.Duration));

		return Task.CompletedTask;
	}

	private static string BuildCacheKey(IActionFilterContext context)
	{
		var request = context.WebContext.Request;

		return $"{request.Method}:{request.Path}{request.QueryString}";
	}

	private sealed class CacheItem(ControllerResponse response, DateTime expirationTimeUtc)
	{
		public ControllerResponse Response { get; } = response;
		public DateTime ExpirationTimeUtc { get; } = expirationTimeUtc;
	}
}
