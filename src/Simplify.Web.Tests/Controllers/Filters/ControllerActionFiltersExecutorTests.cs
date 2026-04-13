using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Simplify.DI;
using Simplify.Web.Attributes;
using Simplify.Web.Controllers;
using Simplify.Web.Controllers.Filters;
using Simplify.Web.Controllers.Meta;
using Simplify.Web.Modules.Context;

namespace Simplify.Web.Tests.Controllers.Filters;

[TestFixture]
public class ControllerActionFiltersExecutorTests
{
	private Mock<IDIResolver> _resolver = null!;
	private Mock<IWebContextProvider> _webContextProvider = null!;

	private DefaultHttpContext _httpContext = null!;

	[SetUp]
	public void Initialize()
	{
		_resolver = new Mock<IDIResolver>();
		_webContextProvider = new Mock<IWebContextProvider>();

		_httpContext = new DefaultHttpContext();
		_httpContext.Request.Method = "GET";
		_httpContext.Request.Path = "/test";
		_httpContext.Request.QueryString = new QueryString("?q=1");

		var webContext = Mock.Of<IWebContext>(x =>
			x.Request == _httpContext.Request &&
			x.Route == "/test");

		_webContextProvider.Setup(x => x.Get()).Returns(webContext);
	}

	[Test]
	public async Task ExecuteAsync_GlobalAndLocalFilters_ExecutedInCorrectOrder()
	{
		// Arrange
		var log = new ExecutionLog();

		_resolver.Setup(x => x.Resolve(It.Is<Type>(t => t == typeof(ExecutionLog))))
			.Returns(log);

		var globalFilters = new List<ActionFilterRegistration>
		{
			new(typeof(GlobalFilter1), 1, true),
			new(typeof(GlobalFilter2), 2, true)
		};

		var localFilters = new List<ActionFilterRegistration>
		{
			new(typeof(LocalFilter1), 1),
			new(typeof(LocalFilter2), 2)
		};

		var executor = CreateExecutor(globalFilters);
		var matchedController = CreateMatchedController(localFilters);

		// Act
		await executor.ExecuteAsync(matchedController, () =>
		{
			log.Steps.Add("controller");
			return Task.FromResult<ControllerResponse?>(null);
		});

		// Assert
		Assert.That(log.Steps, Is.EqualTo(new[]
		{
			"g1.before",
			"g2.before",
			"l1.before",
			"l2.before",
			"controller",
			"l2.after",
			"l1.after",
			"g2.after",
			"g1.after"
		}));
	}

	[Test]
	public async Task ExecuteAsync_FilterStopsExecution_ControllerNotExecuted()
	{
		// Arrange
		var log = new ExecutionLog();
		var expectedResponse = Mock.Of<ControllerResponse>();
		var controllerExecuted = false;

		_resolver.Setup(x => x.Resolve(It.Is<Type>(t => t == typeof(ExecutionLog))))
			.Returns(log);

		var globalFilters = new List<ActionFilterRegistration>
		{
			new(typeof(StopFilter), 0, true, expectedResponse)
		};

		var executor = CreateExecutor(globalFilters);
		var matchedController = CreateMatchedController();

		// Act
		var result = await executor.ExecuteAsync(matchedController, () =>
		{
			controllerExecuted = true;
			return Task.FromResult<ControllerResponse?>(null);
		});

		// Assert
		Assert.That(result, Is.EqualTo(expectedResponse));
		Assert.That(controllerExecuted, Is.False);
		Assert.That(log.Steps, Is.EqualTo(new[] { "stop.before", "stop.after" }));
	}

	[Test]
	public async Task ExecuteAsync_FilterContext_PassedCorrectly()
	{
		// Arrange
		var probe = new ContextProbe();
		var response = Mock.Of<ControllerResponse>();
		var routeParameters = new Dictionary<string, object> { { "id", 10 } };

		_resolver.Setup(x => x.Resolve(It.Is<Type>(t => t == typeof(ContextProbe))))
			.Returns(probe);

		var localFilters = new List<ActionFilterRegistration>
		{
			new(typeof(ContextProbeFilter), 0)
		};

		var executor = CreateExecutor();
		var matchedController = CreateMatchedController(localFilters, routeParameters);

		// Act
		await executor.ExecuteAsync(matchedController, () => Task.FromResult<ControllerResponse?>(response));

		// Assert
		Assert.That(probe.Route, Is.EqualTo("/test"));
		Assert.That(probe.ControllerType, Is.EqualTo(typeof(TestController)));
		Assert.That(probe.RouteParameterId, Is.EqualTo(10));
		Assert.That(probe.ControllerResponseIsSetInOnActionExecuted, Is.True);
	}

	[Test]
	public async Task ExecuteAsync_FilterDependencyInjection_DependencyResolvedThroughResolver()
	{
		// Arrange
		var dependency = new Dependency { Value = "from-di" };
		var probe = new DiProbe();

		_resolver.Setup(x => x.Resolve(It.Is<Type>(t => t == typeof(Dependency))))
			.Returns(dependency);

		_resolver.Setup(x => x.Resolve(It.Is<Type>(t => t == typeof(DiProbe))))
			.Returns(probe);

		var localFilters = new List<ActionFilterRegistration>
		{
			new(typeof(DependencyFilter), 0)
		};

		var executor = CreateExecutor();
		var matchedController = CreateMatchedController(localFilters);

		// Act
		await executor.ExecuteAsync(matchedController, () => Task.FromResult<ControllerResponse?>(null));

		// Assert
		Assert.That(probe.DependencyValue, Is.EqualTo("from-di"));
	}

	[Test]
	public async Task ExecuteAsync_CacheActionFilter_ResponseReturnedFromCache()
	{
		// Arrange
		var runs = 0;
		var response = Mock.Of<ControllerResponse>();
		var queryValue = Guid.NewGuid().ToString("N");

		_httpContext.Request.Path = "/cache-test";
		_httpContext.Request.QueryString = new QueryString($"?k={queryValue}");

		var executor = CreateExecutor(new List<ActionFilterRegistration>
		{
			new(typeof(CacheActionFilter), 0, true, new CacheFilterOptions { Duration = TimeSpan.FromMinutes(5) })
		});

		var matchedController = CreateMatchedController();

		// Act
		var firstResult = await executor.ExecuteAsync(matchedController, () =>
		{
			runs++;
			return Task.FromResult<ControllerResponse?>(response);
		});

		var secondResult = await executor.ExecuteAsync(matchedController, () =>
		{
			runs++;
			return Task.FromResult<ControllerResponse?>(response);
		});

		// Assert
		Assert.That(firstResult, Is.EqualTo(response));
		Assert.That(secondResult, Is.EqualTo(response));
		Assert.That(runs, Is.EqualTo(1));
	}

	private ControllerActionFiltersExecutor CreateExecutor(IReadOnlyList<ActionFilterRegistration>? globalFilters = null) =>
		new(_resolver.Object, _webContextProvider.Object, globalFilters ?? []);

	private static IMatchedController CreateMatchedController(
		IReadOnlyList<ActionFilterRegistration>? localFilters = null,
		IReadOnlyDictionary<string, object>? routeParameters = null)
	{
		var metadata = new Mock<IControllerMetadata>();

		metadata.SetupGet(x => x.ControllerType).Returns(typeof(TestController));
		metadata.SetupGet(x => x.ActionFilters).Returns(localFilters ?? []);

		return new MatchedController(metadata.Object, routeParameters);
	}

	private sealed class TestController;

	private sealed class ExecutionLog
	{
		public IList<string> Steps { get; } = new List<string>();
	}

	[ActionFilter]
	private sealed class GlobalFilter1(ExecutionLog log) : IActionFilter
	{
		public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
		{
			log.Steps.Add("g1.before");
			return Task.FromResult(ActionFilterResult.Continue());
		}

		public Task OnActionExecutedAsync(IActionFilterContext context)
		{
			log.Steps.Add("g1.after");
			return Task.CompletedTask;
		}
	}

	[ActionFilter]
	private sealed class GlobalFilter2(ExecutionLog log) : IActionFilter
	{
		public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
		{
			log.Steps.Add("g2.before");
			return Task.FromResult(ActionFilterResult.Continue());
		}

		public Task OnActionExecutedAsync(IActionFilterContext context)
		{
			log.Steps.Add("g2.after");
			return Task.CompletedTask;
		}
	}

	[ActionFilter]
	private sealed class LocalFilter1(ExecutionLog log) : IActionFilter
	{
		public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
		{
			log.Steps.Add("l1.before");
			return Task.FromResult(ActionFilterResult.Continue());
		}

		public Task OnActionExecutedAsync(IActionFilterContext context)
		{
			log.Steps.Add("l1.after");
			return Task.CompletedTask;
		}
	}

	[ActionFilter]
	private sealed class LocalFilter2(ExecutionLog log) : IActionFilter
	{
		public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
		{
			log.Steps.Add("l2.before");
			return Task.FromResult(ActionFilterResult.Continue());
		}

		public Task OnActionExecutedAsync(IActionFilterContext context)
		{
			log.Steps.Add("l2.after");
			return Task.CompletedTask;
		}
	}

	[ActionFilter]
	private sealed class StopFilter(ExecutionLog log, ControllerResponse response) : IActionFilter
	{
		public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
		{
			log.Steps.Add("stop.before");
			return Task.FromResult(ActionFilterResult.Stop(response));
		}

		public Task OnActionExecutedAsync(IActionFilterContext context)
		{
			log.Steps.Add("stop.after");
			return Task.CompletedTask;
		}
	}

	private sealed class ContextProbe
	{
		public string Route { get; set; } = string.Empty;
		public Type ControllerType { get; set; } = null!;
		public int RouteParameterId { get; set; }
		public bool ControllerResponseIsSetInOnActionExecuted { get; set; }
	}

	[ActionFilter]
	private sealed class ContextProbeFilter(ContextProbe probe) : IActionFilter
	{
		public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
		{
			probe.Route = context.WebContext.Route;
			probe.ControllerType = context.Controller.ControllerType;
			probe.RouteParameterId = (int)context.RouteParameters["id"];

			return Task.FromResult(ActionFilterResult.Continue());
		}

		public Task OnActionExecutedAsync(IActionFilterContext context)
		{
			probe.ControllerResponseIsSetInOnActionExecuted = context.ControllerResponse != null;
			return Task.CompletedTask;
		}
	}

	private sealed class Dependency
	{
		public string Value { get; set; } = string.Empty;
	}

	private sealed class DiProbe
	{
		public string DependencyValue { get; set; } = string.Empty;
	}

	[ActionFilter]
	private sealed class DependencyFilter(Dependency dependency, DiProbe probe) : IActionFilter
	{
		public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
		{
			probe.DependencyValue = dependency.Value;
			return Task.FromResult(ActionFilterResult.Continue());
		}

		public Task OnActionExecutedAsync(IActionFilterContext context) => Task.CompletedTask;
	}
}
