using System;
using System.Collections.Generic;
using NUnit.Framework;
using Simplify.DI.Provider.DryIoc;
using Simplify.Web.Bootstrapper;
using Simplify.Web.Bootstrapper.Setup;
using Simplify.Web.Controllers.Filters;

namespace Simplify.Web.RegistrationsTests;

[TestFixture]
public class BootstrapperActionFiltersRegistrationTests
{
	[Test]
	public void RegisterGlobalActionFilters_CustomFiltersRegistered_OrderedAndOptionsPreserved()
	{
		// Arrange
		var previousProvider = BootstrapperFactory.ContainerProvider;
		var container = new DryIocDIProvider();
		BootstrapperFactory.ContainerProvider = container;
		var cacheOptions = new CacheFilterOptions { Duration = TimeSpan.FromSeconds(15) };
		var bootstrapper = new TestBootstrapper(cacheOptions);

		try
		{
			bootstrapper.RegisterActionFilters();

			// Act
			bootstrapper.RegisterGlobalActionFilters();

			using var scope = container.BeginLifetimeScope();
			var registrations = (IReadOnlyList<ActionFilterRegistration>)scope.Resolver.Resolve(typeof(IReadOnlyList<ActionFilterRegistration>));

			// Assert
			Assert.That(registrations.Count, Is.EqualTo(2));
			Assert.That(registrations[0].FilterType, Is.EqualTo(typeof(CacheActionFilter)));
			Assert.That(registrations[0].Order, Is.EqualTo(1));
			Assert.That(registrations[0].IsGlobal, Is.True);
			Assert.That(registrations[0].Options, Is.SameAs(cacheOptions));
			Assert.That(registrations[1].FilterType, Is.EqualTo(typeof(LoggingActionFilter)));
			Assert.That(registrations[1].Order, Is.EqualTo(2));
			Assert.That(registrations[1].IsGlobal, Is.True);
		}
		finally
		{
			BootstrapperFactory.ContainerProvider = previousProvider;
		}
	}

	[Test]
	public void RegisterGlobalActionFilters_NoCustomFilters_EmptyCollectionRegistered()
	{
		// Arrange
		var previousProvider = BootstrapperFactory.ContainerProvider;
		var container = new DryIocDIProvider();
		BootstrapperFactory.ContainerProvider = container;
		var bootstrapper = new BaseBootstrapper();

		try
		{
			// Act
			bootstrapper.RegisterGlobalActionFilters();

			using var scope = container.BeginLifetimeScope();
			var registrations = (IReadOnlyList<ActionFilterRegistration>)scope.Resolver.Resolve(typeof(IReadOnlyList<ActionFilterRegistration>));

			// Assert
			Assert.That(registrations, Is.Empty);
		}
		finally
		{
			BootstrapperFactory.ContainerProvider = previousProvider;
		}
	}

	private sealed class TestBootstrapper(CacheFilterOptions cacheOptions) : BaseBootstrapper
	{
		public override void RegisterActionFilters()
		{
			RegisterGlobalFilter<LoggingActionFilter>(order: 2);
			RegisterGlobalFilter<CacheActionFilter>(cacheOptions, order: 1);
		}
	}
}


