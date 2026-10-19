# Diagrama de clases — Capa WebApi

Controllers (REST), filtros de autorización y manejo de excepciones.
Cada controller depende de una **interfaz de servicio** (capa `IBussinesLogic`) — no de implementaciones concretas.

```mermaid
classDiagram
    direction LR

    class ControllerBase
    <<abstract>> ControllerBase

    %% ---------- Controllers ----------
    class SessionController {
        +Login(LoginDto) IActionResult
        +Logout() IActionResult
    }
    class UserController {
        +Create(UpdateUserDto) IActionResult
        +Register(RegisterUserDto) IActionResult
        +SearchBy(string, string) IActionResult
        +GetUserById(Guid) IActionResult
        +UpdateUser(Guid, UpdateUserDto) IActionResult
        +DeleteUser(Guid) IActionResult
        -GetToken() Guid
    }
    class ProductController {
        +CreateProduct(CreateProductRequestDto) IActionResult
        +UpdateProduct(Guid, UpdateProductRequestDto) IActionResult
        +SearchProducts(SearchProductRequestDto) IActionResult
        +GetProduct(Guid) IActionResult
        +ChangeProductActivation(Guid) IActionResult
        -GetToken() Guid
    }
    class PromotionController {
        +GetActivePromotions(SearchPromotionRequestDto) IActionResult
        +CreatePromotion(CreatePromotionRequestDto) IActionResult
        +UpdatePromotion(Guid, UpdatePromotionRequestDto) IActionResult
        +AddProductToPromotion(Guid, AddProductToPromotionDto) IActionResult
        +RemoveProductFromPromotion(Guid, Guid) IActionResult
        +GetCommercialLines() IActionResult
        -GetToken() Guid
    }
    class OrderController {
        +CreateOrder(CreateOrderRequestDto) IActionResult
        +GetOrderById(Guid) IActionResult
        +ExecuteAction(Guid, OrderActionRequestDto) IActionResult
        +GetCustomerOrders(SearchCustomerOrdersRequestDto) IActionResult
        +GetFilteredOrders(SearchFilteredOrdersRequestDto) IActionResult
    }
    class ShippingTypeController {
        -IShippingTypeService _shippingTypeService
        +Create(CreateShippingTypeRequestDto) IActionResult
        +Update(Guid, UpdateShippingTypeRequestDto) IActionResult
        +GetAll() IActionResult
        +Delete(Guid) IActionResult
    }
    class ReportController {
        +GetTopProducts(DateTime, DateTime) IActionResult
        +GetSalesReport(DateTime, DateTime) IActionResult
    }
    class AuditController {
        +Search(SearchAuditRequestDto) IActionResult
    }
    class ImporterController {
        +GetImporterNames() IActionResult
        +ImportProducts(string, string) IActionResult
    }

    %% ---------- Service interfaces (IBussinesLogic) ----------
    class ISessionService
    class IUserService
    class IProductService
    class IPromotionService
    class IOrderService
    class IShippingTypeService
    class IReportService
    class IAuditLoggerService
    class IImporterService
    <<interface>> ISessionService
    <<interface>> IUserService
    <<interface>> IProductService
    <<interface>> IPromotionService
    <<interface>> IOrderService
    <<interface>> IShippingTypeService
    <<interface>> IReportService
    <<interface>> IAuditLoggerService
    <<interface>> IImporterService

    %% ---------- Cross-cutting (Authorization / Exceptions) ----------
    class IAuthorizationFilter
    class IExceptionFilter
    class TypeFilterAttribute
    <<interface>> IAuthorizationFilter
    <<interface>> IExceptionFilter

    class AuthorizationFilter {
        -HashSet~string~ _roles
        -ISessionRepository _sessionRepository
        +AuthorizationFilter(Role[] roles, ISessionRepository)
        +OnAuthorization(AuthorizationFilterContext) void
    }
    class AuthorizationFilterAttribute {
        +AuthorizationFilterAttribute(Role[] roles)
    }
    class ExceptionFilter {
        +OnException(ExceptionContext) void
    }
    class ISessionRepository
    <<interface>> ISessionRepository

    %% ---------- Herencia ----------
    SessionController --|> ControllerBase
    UserController --|> ControllerBase
    ProductController --|> ControllerBase
    PromotionController --|> ControllerBase
    OrderController --|> ControllerBase
    ShippingTypeController --|> ControllerBase
    ReportController --|> ControllerBase
    AuditController --|> ControllerBase
    ImporterController --|> ControllerBase

    %% ---------- Dependencias controller -> servicio ----------
    SessionController ..> ISessionService
    UserController ..> IUserService
    ProductController ..> IProductService
    PromotionController ..> IPromotionService
    OrderController ..> IOrderService
    ShippingTypeController ..> IShippingTypeService
    ReportController ..> IReportService
    AuditController ..> IAuditLoggerService
    ImporterController ..> IImporterService

    %% ---------- Filtros ----------
    AuthorizationFilter ..|> IAuthorizationFilter
    ExceptionFilter ..|> IExceptionFilter
    AuthorizationFilterAttribute --|> TypeFilterAttribute
    AuthorizationFilterAttribute ..> AuthorizationFilter : instancia (TypeFilter)
    AuthorizationFilter ..> ISessionRepository : valida token
```

> **Notas**
> - Todos los controllers se decoran con `[AuthorizationFilter([roles...])]` para proteger sus endpoints por rol; `[]` = solo requiere sesión válida.
> - `ExceptionFilter` se registra **globalmente** en `Program.cs` (`options.Filters.Add<ExceptionFilter>()`) y mapea excepciones de dominio a códigos HTTP (`ConflictException`→409, `ArgumentException`→400, `InvalidOperationException`→422, `KeyNotFoundException`→404, `UnauthorizedAccessException`→401).
> - `Program.cs` es la *composition root*: configura CORS, registra los servicios (vía `ServiceFactory`) y los filtros.
```
