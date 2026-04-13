using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Simplify.DI;
using Simplify.Web.Modules.Context;

namespace Simplify.Web.Controllers.Filters;

/// <summary>
/// Provides a controller action filters executor.
/// </summary>
/// <param name="resolver">The DI resolver.</param>
/// <param name="webContextProvider">The web context provider.</param>
/// <param name="globalFilters">The global action filters.</param>
public class ControllerActionFiltersExecutor(
	IDIResolver resolver,
	IWebContextProvider webContextProvider,
	IReadOnlyList<ActionFilterRegistration> globalFilters) : IControllerActionFiltersExecutor
{
	private static readonly IReadOnlyDictionary<string, object> EmptyRouteParameters = new Dictionary<string, object>();

	/// <summary>
	/// Executes controller action filters and controller action itself.
	/// </summary>
	/// <param name="matchedController">The matched controller.</param>
	/// <param name="controllerExecution">The controller execution delegate.</param>
	public async Task<ControllerResponse?> ExecuteAsync(IMatchedController matchedController, Func<Task<ControllerResponse?>> controllerExecution)
	{
		var orderedFilters = GetFiltersInExecutionOrder(matchedController).ToList();

		if (orderedFilters.Count == 0)
			return await controllerExecution();

		var context = new ActionFilterContext(
			webContextProvider.Get(),
			matchedController.Controller,
			matchedController.RouteParameters ?? EmptyRouteParameters);

		var executedFilters = new List<IActionFilter>(orderedFilters.Count);
		var shouldExecuteController = true;

		foreach (var registration in orderedFilters)
		{
			var filter = CreateFilter(registration);

			executedFilters.Add(filter);

			var result = await filter.OnActionExecutingAsync(context);

			if (!result.IsExecutionStopped)
				continue;

			shouldExecuteController = false;
			context.ControllerResponse = result.ControllerResponse;
			break;
		}

		if (shouldExecuteController)
			context.ControllerResponse = await controllerExecution();

		for (var i = executedFilters.Count - 1; i >= 0; i--)
			await executedFilters[i].OnActionExecutedAsync(context);

		return context.ControllerResponse;
	}

	private IEnumerable<ActionFilterRegistration> GetFiltersInExecutionOrder(IMatchedController matchedController)
	{
		var globalOrdered = globalFilters
			.Where(x => x.IsGlobal)
			.OrderBy(x => x.Order);

		var localOrdered = matchedController.Controller.ActionFilters
			.Where(x => !x.IsGlobal)
			.OrderBy(x => x.Order);

		return globalOrdered.Concat(localOrdered);
	}

	private IActionFilter CreateFilter(ActionFilterRegistration registration)
	{
		var instance = CreateTypeInstance(registration.FilterType, registration.Options);

		if (instance is IActionFilter filter)
			return filter;

		throw new InvalidOperationException($"Action filter should implement interface '{nameof(IActionFilter)}', actual type: '{registration.FilterType.FullName}'.");
	}

	private object CreateTypeInstance(Type type, object? options)
	{
		var constructors = type
			.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
			.OrderByDescending(x => x.GetParameters().Length)
			.ToList();
		Exception? lastException = null;

		if (constructors.Count == 0)
			throw new InvalidOperationException($"No public constructors found for action filter type '{type.FullName}'.");

		foreach (var constructor in constructors)
		{
			var parameters = constructor.GetParameters();

			if (options != null && parameters.All(x => !x.ParameterType.IsInstanceOfType(options)))
				continue;

			try
			{
				var args = BuildConstructorParameters(parameters, options);

				return constructor.Invoke(args);
			}
			catch (Exception e)
			{
				lastException = e;
				// Try next constructor.
			}
		}

		throw new InvalidOperationException($"Unable to instantiate action filter type '{type.FullName}'.", lastException);
	}

	private object?[] BuildConstructorParameters(IReadOnlyList<ParameterInfo> parameters, object? options)
	{
		var args = new object?[parameters.Count];
		var optionsIsSet = false;

		for (var i = 0; i < parameters.Count; i++)
		{
			var parameter = parameters[i];

			if (!optionsIsSet && options != null && parameter.ParameterType.IsInstanceOfType(options))
			{
				args[i] = options;
				optionsIsSet = true;
				continue;
			}

			args[i] = resolver.Resolve(parameter.ParameterType);
		}

		return args;
	}
}
