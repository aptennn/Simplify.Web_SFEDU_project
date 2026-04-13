using System;
using System.Threading.Tasks;
using Simplify.Web.Attributes;
using Simplify.Web.Controllers.Filters;

namespace Simplify.Web.Tests.Controllers.V1.Metadata.MetadataFactoryTests.TestTypes;

[Get("/filtered")]
[ApplyFilter(typeof(SecondFilter), Order = 2)]
[ApplyFilter(typeof(FirstFilter), Order = 1)]
public class FilteredController : Controller
{
	public override ControllerResponse Invoke() => throw new NotImplementedException();
}

[ActionFilter]
public class FirstFilter : IActionFilter
{
	public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context) => Task.FromResult(ActionFilterResult.Continue());
	public Task OnActionExecutedAsync(IActionFilterContext context) => Task.CompletedTask;
}

[ActionFilter]
public class SecondFilter : IActionFilter
{
	public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context) => Task.FromResult(ActionFilterResult.Continue());
	public Task OnActionExecutedAsync(IActionFilterContext context) => Task.CompletedTask;
}
