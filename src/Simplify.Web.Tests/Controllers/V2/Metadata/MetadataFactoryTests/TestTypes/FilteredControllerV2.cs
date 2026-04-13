using System;
using System.Threading.Tasks;
using Simplify.Web.Attributes;
using Simplify.Web.Controllers.Filters;

namespace Simplify.Web.Tests.Controllers.V2.Metadata.MetadataFactoryTests.TestTypes;

[Get("/filtered-v2")]
[ApplyFilter(typeof(V2SecondFilter), Order = 2)]
[ApplyFilter(typeof(V2FirstFilter), Order = 1)]
public class FilteredControllerV2 : Controller2
{
	public ControllerResponse Invoke() => throw new NotImplementedException();
}

[ActionFilter]
public class V2FirstFilter : IActionFilter
{
	public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context) => Task.FromResult(ActionFilterResult.Continue());
	public Task OnActionExecutedAsync(IActionFilterContext context) => Task.CompletedTask;
}

[ActionFilter]
public class V2SecondFilter : IActionFilter
{
	public Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context) => Task.FromResult(ActionFilterResult.Continue());
	public Task OnActionExecutedAsync(IActionFilterContext context) => Task.CompletedTask;
}
