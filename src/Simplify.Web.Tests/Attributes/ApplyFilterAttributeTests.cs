using System;
using NUnit.Framework;
using Simplify.Web.Attributes;
using Simplify.Web.Controllers.Filters;

namespace Simplify.Web.Tests.Attributes;

[TestFixture]
public class ApplyFilterAttributeTests
{
	[Test]
	public void Constructor_FilterTypeDoesNotImplementIActionFilter_ThrowsArgumentException()
	{
		// Act
		Assert.Throws<ArgumentException>(() => new ApplyFilterAttribute(typeof(string)));
	}

	[Test]
	public void Constructor_FilterTypeImplementsIActionFilter_PropertiesSet()
	{
		// Act
		var attr = new ApplyFilterAttribute(typeof(TestFilter)) { Order = 10 };

		// Assert
		Assert.That(attr.FilterType, Is.EqualTo(typeof(TestFilter)));
		Assert.That(attr.Order, Is.EqualTo(10));
	}

	private sealed class TestFilter : IActionFilter
	{
		public global::System.Threading.Tasks.Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context) =>
			global::System.Threading.Tasks.Task.FromResult(ActionFilterResult.Continue());

		public global::System.Threading.Tasks.Task OnActionExecutedAsync(IActionFilterContext context) =>
			global::System.Threading.Tasks.Task.CompletedTask;
	}
}
