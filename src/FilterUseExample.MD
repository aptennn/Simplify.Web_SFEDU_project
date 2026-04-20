# Примеры использования фильтров.

##  Пользовательский фильтр
```chasrp
[ActionFilter]
public class AuthenticationFilter : IActionFilter
{
    private readonly IAuthService _authService;
    
    public AuthenticationFilter(IAuthService authService)
    {
        _authService = authService;
    }
    
    public async Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
    {
        var token = context.WebContext.Request.Headers["Authorization"].ToString();
        
        if (!_authService.IsValidToken(token))
        {
            return ActionFilterResult.Stop(
                new Unauthorized("Invalid token"));
        }
        
        return ActionFilterResult.Continue();
    }
    
    public Task OnActionExecutedAsync(IActionFilterContext context) => Task.CompletedTask;
}
```
## Применение фильтра к контроллеру
```csharp


[ApplyFilter(typeof(AuthenticationFilter), Order = 1)]
[ApplyFilter(typeof(LoggingActionFilter), Order = 2)]
[Get("/api/users")]
public class UsersController : Controller2
{
    public ControllerResponse GetUsers() => new Json(GetAllUsers());
}
```

##  Глобальная регистрация
```csharp
public class MyBootstrapper : BaseBootstrapper
{
    public override void RegisterActionFilters()
    {
        // Применяется ко всем контроллерам
        RegisterGlobalFilter<LoggingActionFilter>(order: 1);
        
        RegisterGlobalFilter<CacheActionFilter>(
            new CacheFilterOptions { Duration = TimeSpan.FromMinutes(10) },
            order: 2);
    }
}
```