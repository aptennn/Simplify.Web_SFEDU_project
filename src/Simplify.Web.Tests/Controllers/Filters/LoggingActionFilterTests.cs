using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Simplify.Web.Controllers.Filters;
using Simplify.Web.Controllers.Meta;
using Simplify.Web.Modules.Context;

namespace Simplify.Web.Tests.Controllers.Filters;

[TestFixture]
public class LoggingActionFilterTests
{
	[Test]
	public async Task OnActionExecutingAsync_Always_StartsStopwatchAndContinuesExecution()
	{
		// Arrange
		var filter = new LoggingActionFilter();
		var context = CreateContext();

		// Act
		var result = await filter.OnActionExecutingAsync(context);
		var stopwatch = context.Items.Values.OfType<Stopwatch>().SingleOrDefault();

		// Assert
		Assert.That(result.IsExecutionStopped, Is.False);
		Assert.That(result.ControllerResponse, Is.Null);
		Assert.That(stopwatch, Is.Not.Null);
		Assert.That(stopwatch!.IsRunning, Is.True);
	}

	[Test]
	public async Task OnActionExecutedAsync_StopwatchInContext_StopwatchStopped()
	{
		// Arrange
		var filter = new LoggingActionFilter();
		var context = CreateContext();
		await filter.OnActionExecutingAsync(context);
		var stopwatch = context.Items.Values.OfType<Stopwatch>().Single();

		// Act
		await filter.OnActionExecutedAsync(context);

		// Assert
		Assert.That(stopwatch.IsRunning, Is.False);
	}

	[Test]
	public void OnActionExecutedAsync_StopwatchNotFound_CompletesWithoutExceptions()
	{
		// Arrange
		var filter = new LoggingActionFilter();
		var context = CreateContext();

		// Assert
		Assert.DoesNotThrowAsync(async () => await filter.OnActionExecutedAsync(context));
	}

	private static ActionFilterContext CreateContext()
	{
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Path = "/log-filter-test";

		var webContext = Mock.Of<IWebContext>(x =>
			x.Request == httpContext.Request &&
			x.Route == "/log-filter-test");
		var controllerMetadata = Mock.Of<IControllerMetadata>(x => x.ControllerType == typeof(TestController));

		return new ActionFilterContext(webContext, controllerMetadata, new Dictionary<string, object>());
	}

	private sealed class TestController;
}

