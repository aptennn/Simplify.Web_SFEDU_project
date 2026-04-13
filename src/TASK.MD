Реализация системы Action Filters для контроллеров

Добавьте в фреймворк Simplify.Web систему фильтров (Action Filters), которая позволит выполнять код до и после выполнения контроллеров, а также прерывать выполнение контроллера и возвращать собственный ответ. Система фильтров должна поддерживать внедрение зависимостей через конструктор и позволять применять фильтры как к отдельным контроллерам через атрибуты, так и глобально через Bootstrapper.

Проект на GitHub: https://github.com/SimplifyNet/Simplify.Web
Требования к реализации

Создайте следующие компоненты системы фильтров:

    Интерфейсы фильтров:
        IActionFilter - базовый интерфейс для всех фильтров с методами OnActionExecutingAsync и OnActionExecutedAsync
        IActionFilterContext - контекст выполнения фильтра, содержащий информацию о текущем запросе, маршруте, параметрах и ControllerResponse
        IActionFilterResult - результат выполнения фильтра, позволяющий продолжить выполнение или прервать его с определенным ответом
    Атрибуты для применения фильтров:
        ActionFilterAttribute - базовый атрибут для маркировки фильтров
        ApplyFilterAttribute - атрибут для применения фильтров к контроллерам, поддерживающий указание порядка выполнения
    Механизм выполнения фильтров:
        Модифицируйте Controller1Executor и Controller2Executor для вызова фильтров перед и после выполнения метода контроллера
        Фильтры должны выполняться в порядке, указанном в их приоритете (сначала глобальные, затем примененные через атрибуты)
        Если фильтр возвращает ControllerResponse в методе OnActionExecutingAsync, выполнение контроллера должно быть прервано и возвращен этот ответ
    Интеграция с метаданными контроллеров:
        Расширьте IControllerMetadata для хранения списка фильтров, примененных к контроллеру
        Модифицируйте Controller1MetadataFactory и Controller2MetadataFactory для извлечения информации о фильтрах из атрибутов контроллера
    Регистрация в Bootstrapper:
        Добавьте методы в BaseBootstrapper для регистрации глобальных фильтров, применяемых ко всем контроллерам
        Обеспечьте поддержку DI для фильтров через существующий контейнер зависимостей
    Готовые реализации фильтров:
        LoggingActionFilter - фильтр для логирования входящих запросов и времени выполнения контроллера
        CacheActionFilter - фильтр для кэширования результатов контроллеров с указанием времени жизни кэша

Пример использования

После реализации системы разработчики должны иметь возможность использовать фильтры следующим образом:

// Создание пользовательского фильтра
public class ValidationFilter : IActionFilter
{
public async Task<IActionFilterResult> OnActionExecutingAsync(IActionFilterContext context)
{
// Выполняется перед контроллером
if (!context.ModelState.IsValid)
{
return ActionFilterResult.Stop(new Json(new { errors = context.ModelState.Errors }));
}
return ActionFilterResult.Continue();
}

    public async Task OnActionExecutedAsync(IActionFilterContext context)
    {
        // Выполняется после контроллера
        // Можно модифицировать ответ в контексте
    }
}

// Применение фильтра к контроллеру
[ApplyFilter(typeof(ValidationFilter), Order = 1)]
[ApplyFilter(typeof(LoggingFilter), Order = 2)]
public class MyController : Controller2
{
[Get("/api/data")]
public async Task<ControllerResponse> GetData()
{
// Логика контроллера
}
}

// Глобальная регистрация фильтров в Startup.cs
public class MyBootstrapper : BaseBootstrapper
{
public override void RegisterActionFilters()
{
RegisterGlobalFilter<LoggingFilter>();
RegisterGlobalFilter<CacheFilter>(new CacheFilterOptions { Duration = TimeSpan.FromMinutes(5) });
}
}

Тестирование

Напишите unit-тесты для:

    Проверки порядка выполнения фильтров
    Проверки прерывания выполнения контроллера фильтром
    Проверки передачи контекста в фильтры
    Проверки DI в фильтрах
    Проверки взаимодействия с Controller1 и Controller2

Критерии успешной реализации

    Все существующие тесты проходят
    Новые тесты для системы фильтров проходят
    Фильтры корректно работают с контроллерами V1 и V2
    Поддерживается DI через конструктор в фильтрах
    Глобальные фильтры применяются ко всем контроллерам
    Фильтры, примененные через атрибуты, работают с учетом указанного порядка
    Примеры фильтров (LoggingFilter и CacheFilter) работают корректно
