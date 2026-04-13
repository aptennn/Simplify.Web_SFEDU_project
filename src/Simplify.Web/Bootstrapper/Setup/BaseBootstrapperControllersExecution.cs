using System.Collections.Generic;
using System.Linq;
using Simplify.DI;
using Simplify.Web.Controllers.Execution;
using Simplify.Web.Controllers.Execution.Resolver;
using Simplify.Web.Controllers.Filters;
using Simplify.Web.Controllers.Response;
using Simplify.Web.Controllers.V1.Execution;
using Simplify.Web.Controllers.V2.Execution;
using Simplify.Web.Modules.Redirection;

namespace Simplify.Web.Bootstrapper.Setup;

/// <summary>
/// Provides the bootstrapper controllers execution registration.
/// </summary>
public partial class BaseBootstrapper
{
	/// <summary>
	/// Registers the global action filters.
	/// </summary>
	public virtual void RegisterGlobalActionFilters()
	{
		if (TypesToExclude.Contains(typeof(IReadOnlyList<ActionFilterRegistration>)))
			return;

		BootstrapperFactory.ContainerProvider.Register<IReadOnlyList<ActionFilterRegistration>>(_ =>
			_globalActionFilters
				.OrderBy(x => x.Order)
				.ToList()
				.AsReadOnly(),
			LifetimeType.Singleton);
	}

	/// <summary>
	/// Registers the controller action filters executor.
	/// </summary>
	public virtual void RegisterControllerActionFiltersExecutor()
	{
		if (TypesToExclude.Contains(typeof(IControllerActionFiltersExecutor)))
			return;

		BootstrapperFactory.ContainerProvider.Register<IControllerActionFiltersExecutor, ControllerActionFiltersExecutor>();
	}

	/// <summary>
	/// Registers the controller executor resolver.
	/// </summary>
	public virtual void RegisterControllerExecutorResolver()
	{
		if (TypesToExclude.Contains(typeof(IControllerExecutorResolver)))
			return;

		BootstrapperFactory.ContainerProvider.Register<IControllerExecutorResolver, ControllerExecutorResolver>();
	}

	/// <summary>
	/// Registers the controller executor resolver executors.
	/// </summary>
	public virtual void RegisterControllerExecutorResolverExecutors()
	{
		if (TypesToExclude.Contains(typeof(IReadOnlyList<IControllerExecutor>)))
			return;

		BootstrapperFactory.ContainerProvider.Register<IReadOnlyList<IControllerExecutor>>(r =>
			[
				new Controller2Executor(r.Resolve<IController2Factory>(), r.Resolve<IControllerActionFiltersExecutor>()),
				new Controller1Executor(r.Resolve<IController1Factory>(), r.Resolve<IControllerActionFiltersExecutor>())
			]);
	}

	/// <summary>
	/// Registers the controller response executor.
	/// </summary>
	public virtual void RegisterControllerResponseExecutor()
	{
		if (TypesToExclude.Contains(typeof(IControllerResponseExecutor)))
			return;

		BootstrapperFactory.ContainerProvider.Register<IControllerResponseExecutor>(r => new ControllerResponseExecutor(r));
	}

	/// <summary>
	/// Registers the controllers executor.
	/// </summary>
	public virtual void RegisterControllersExecutor()
	{
		if (TypesToExclude.Contains(typeof(IControllersExecutor)))
			return;

		BootstrapperFactory.ContainerProvider.Register<ControllersExecutor>();

		BootstrapperFactory.ContainerProvider.Register<IControllersExecutor>(r =>
			new PreviousPageUrlUpdater(
					r.Resolve<ControllersExecutor>(),
				r.Resolve<IRedirector>()));
	}
}
