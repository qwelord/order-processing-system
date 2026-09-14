services:
  postgres-orders:
    image: postgres:15-alpine
    container_name: postgres-orders
    environment:
      POSTGRES_DB: orders_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgrespassword
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d orders_db"]
      interval: 5s
      timeout: 5s
      retries: 5

  postgres-payments:
    image: postgres:15-alpine
    container_name: postgres-payments
    environment:
      POSTGRES_DB: payments_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgrespassword
    ports:
      - "5433:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d payments_db"]
      interval: 5s
      timeout: 5s
      retries: 5

  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    container_name: zookeeper
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    container_name: kafka
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:29092,PLAINTEXT_HOST://localhost:9092
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
      KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
    healthcheck:
      test: ["CMD", "nc", "-z", "localhost", "29092"]
      interval: 5s
      timeout: 5s
      retries: 5

  payment-service:
    build:
      context: .
      dockerfile: PaymentService.WebApi/Dockerfile
    container_name: payment-service
    ports:
      - "5002:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres-payments;Port=5432;Database=payments_db;Username=postgres;Password=postgrespassword
      - Kafka__BootstrapServers=kafka:29092
    depends_on:
      postgres-payments:
        condition: service_healthy
      kafka:
        condition: service_started

  order-service:
    build:
      context: .
      dockerfile: OrderService.WebApi/Dockerfile
    container_name: order-service
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres-orders;Port=5432;Database=orders_db;Username=postgres;Password=postgrespassword
      - PaymentService__BaseUrl=http://payment-service:8080
    depends_on:
      postgres-orders:
        condition: service_healthy
      payment-service:
        condition: service_started
      kafka:
        condition: service_started

  notification-service:
    build:
      context: .
      dockerfile: NotificationService.WebApi/Dockerfile
    container_name: notification-service
    ports:
      - "5003:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Kafka__BootstrapServers=kafka:29092
    depends_on:
      kafka:
        condition: service_healthy

начинай и делай до первого коммита, текст кода прикрепляю ниже
<!DOCTYPE html><html lang="ru"><head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Order Processing System — Live Event Monitor</title>
    <style>
        :root {
            --bg-color: #0f172a;
            --card-bg: #1e293b;
            --text-primary: #f8fafc;
            --text-secondary: #94a3b8;
            --accent-green: #10b981;
            --accent-red: #ef4444;
            --accent-blue: #3b82f6;
            --border-color: #334155;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
            background-color: var(--bg-color);
            color: var(--text-primary);
            margin: 0;
            padding: 40px 20px;
            display: flex;
            justify-content: center;
        }

        .container {
            width: 100%;
            max-width: 800px;
        }

        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 24px;
            padding-bottom: 16px;
            border-bottom: 1px solid var(--border-color);
        }

        .title {
            font-size: 20px;
            font-weight: 600;
            letter-spacing: -0.02em;
            margin: 0;
        }

        .status-badge {
            font-size: 13px;
            font-weight: 500;
            padding: 6px 12px;
            border-radius: 9999px;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            border: 1px solid transparent;
        }

            .status-badge.connected {
                background-color: rgba(16, 185, 129, 0.1);
                color: var(--accent-green);
                border-color: rgba(16, 185, 129, 0.2);
            }

            .status-badge.disconnected {
                background-color: rgba(239, 68, 68, 0.1);
                color: var(--accent-red);
                border-color: rgba(239, 68, 68, 0.2);
            }

        .status-dot {
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background-color: currentColor;
        }

        .events-container {
            display: flex;
            flex-direction: column;
            gap: 12px;
        }

        .empty-state {
            text-align: center;
            padding: 48px;
            background-color: var(--card-bg);
            border-radius: 8px;
            border: 1px dashed var(--border-color);
            color: var(--text-secondary);
            font-size: 14px;
        }

        .event-card {
            background-color: var(--card-bg);
            border: 1px solid var(--border-color);
            border-radius: 8px;
            padding: 16px;
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 12px;
            animation: fadeIn 0.3s ease-in-out;
        }

        .event-field {
            display: flex;
            flex-direction: column;
            gap: 4px;
        }

        .event-label {
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            color: var(--text-secondary);
            font-weight: 600;
        }

        .event-value {
            font-size: 14px;
            font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
            color: var(--text-primary);
        }

        .status-completed {
            color: var(--accent-green);
            font-weight: 600;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(-8px);
            }

            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
    </style></head><body>
    <div class="container">
        <div class="header">
            <h1 class="title">SignalR Live Stream Monitor</h1>
            <div id="statusBadge" class="status-badge disconnected">
                <span class="status-dot"></span>
                <span id="statusText">Disconnected</span>
            </div>
        </div>

        <div id="eventsContainer" class="events-container">
            <div id="emptyState" class="empty-state">
                Awaiting incoming payment events from Kafka...
            </div>
        </div>
    </div>

    <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.min.js"></script>
    <script>
        const statusBadge = document.getElementById('statusBadge');
        const statusText = document.getElementById('statusText');
        const eventsContainer = document.getElementById('eventsContainer');
        const emptyState = document.getElementById('emptyState');

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5003/notifications")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceivePaymentUpdate", (data) => {
            if (emptyState) {
                emptyState.style.display = 'none';
            }

            const card = document.createElement('div');
            card.className = 'event-card';

            const formattedAmount = new Intl.NumberFormat('ru-RU', { style: 'currency', currency: 'RUB' }).format(data.amount);
            const formattedDate = new Date(data.timestamp).toLocaleTimeString('ru-RU');

            card.innerHTML = `
                    <div class="event-field">
                        <span class="event-label">Order ID</span>
                        <span class="event-value">${data.orderId}</span>
                    </div>
                    <div class="event-field">
                        <span class="event-label">Payment ID</span>
                        <span class="event-value">${data.paymentId}</span>
                    </div>
                    <div class="event-field">
                        <span class="event-label">Amount</span>
                        <span class="event-value">${formattedAmount}</span>
                    </div>
                    <div class="event-field">
                        <span class="event-label">Status / Time</span>
                        <span class="event-value status-completed">${data.status} (${formattedDate})</span>
                    </div>
                `;

            eventsContainer.prepend(card);
        });

        connection.start()
            .then(() => {
                statusBadge.className = "status-badge connected";
                statusText.textContent = "Connected";
            })
            .catch(err => {
                statusBadge.className = "status-badge disconnected";
                statusText.textContent = "Connection Error";
            });
    </script></body></html>
namespace NotificationService.WebApi.Hubs;public interface INotificationClient{
    Task ReceivePaymentUpdate(PaymentNotificationContract notification);}public record PaymentNotificationContract(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    string Status,
    DateTime Timestamp);
using Microsoft.AspNetCore.SignalR;namespace NotificationService.WebApi.Hubs;public class NotificationHub : Hub<INotificationClient>{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }}
using Confluent.Kafka;using Microsoft.AspNetCore.SignalR;using NotificationService.WebApi.Hubs;using System.Text.Json;namespace NotificationService.WebApi.Services;public class KafkaConsumerService : BackgroundService{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _topic;

    public KafkaConsumerService(
        IHubContext<NotificationHub, INotificationClient> hubContext,
        IConfiguration configuration,
        ILogger<KafkaConsumerService> logger)
    {
        _hubContext = hubContext;
        _configuration = configuration;
        _logger = logger;
        _topic = _configuration["Kafka:Topic"] ?? "payment-events";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "kafka:9092";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "notification-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = false
        };

        _logger.LogInformation("Kafka Consumer initializing with BootstrapServers: {BootstrapServers}", bootstrapServers);

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, error) => _logger.LogError("Kafka Consumer Error: {Reason} (Code: {Code})", error.Reason, error.Code))
            .Build();

        consumer.Subscribe(_topic);
        _logger.LogInformation("Kafka Consumer subscribed to topic: {Topic}", _topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);

                if (consumeResult?.Message == null) continue;

                _logger.LogInformation("Consumed message with Key: {Key} from Partition: {Partition}",
                    consumeResult.Message.Key, consumeResult.Partition.Value);

                var notification = JsonSerializer.Deserialize<PaymentNotificationContract>(
                    consumeResult.Message.Value,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (notification != null)
                {
                    await _hubContext.Clients.All.ReceivePaymentUpdate(notification);
                }

                consumer.Commit(consumeResult);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error occurred while consuming Kafka message: {Reason}", ex.Error.Reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing Kafka event");
            }
        }

        consumer.Close();
        _logger.LogInformation("Kafka Consumer connection closed gracefully.");
    }}
using NotificationService.WebApi.Hubs;using NotificationService.WebApi.Services;var builder = WebApplication.CreateBuilder(args);builder.Services.AddCors(options =>{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });});builder.Services.AddSignalR();builder.Services.AddHostedService<KafkaConsumerService>();builder.Services.AddEndpointsApiExplorer();builder.Services.AddSwaggerGen();var app = builder.Build();app.UseSwagger();app.UseSwaggerUI(c =>{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API v1");
    c.RoutePrefix = "swagger";});app.UseCors("AllowAll");app.UseDefaultFiles();app.UseStaticFiles();app.MapHub<NotificationHub>("/notifications");app.Run();
using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore.Metadata.Builders;using OrderService.DataAccess.Entities;namespace OrderService.DataAccess.Configurations;public class OrderConfiguration : IEntityTypeConfiguration<Order>{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .HasConversion<int>();
    }}
namespace OrderService.DataAccess.Entities;public enum OrderStatus{
    Created = 0,
    PendingPayment = 1,
    Paid = 2,
    Failed = 3}public class Order{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }}
using Microsoft.EntityFrameworkCore;using OrderService.DataAccess.Entities;using System.Reflection.Emit;namespace OrderService.DataAccess;public class OrderDbContext : DbContext{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }}
using AutoMapper;using Microsoft.EntityFrameworkCore;using NUnit.Framework;using OrderService.DataAccess;using OrderService.DataAccess.Entities;using OrderService.WebApi.DTOs;using OrderService.WebApi.Mappings;using OrderService.WebApi.UseCases.Commands;namespace OrderService.Tests;

[TestFixture]public class CreateOrderCommandHandlerTests{
    private OrderDbContext _dbContext = null!;
    private IMapper _mapper = null!;
    private CreateOrderCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new OrderDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _handler = new CreateOrderCommandHandler(_dbContext, _mapper);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task Handle_ShouldSaveOrderToDatabaseAndReturnMappedDto()
    {
        var dto = new CreateOrderDto("Алексей", 1500m);
        var command = new CreateOrderCommand(dto);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.CustomerName, Is.EqualTo("Алексей"));
        Assert.That(result.TotalAmount, Is.EqualTo(1500m));
        Assert.That(result.Status, Is.EqualTo(OrderStatus.Created.ToString()));

        var savedOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == result.Id);
        Assert.That(savedOrder, Is.Not.Null);
        Assert.That(savedOrder!.CustomerName, Is.EqualTo("Алексей"));
        Assert.That(savedOrder.TotalAmount, Is.EqualTo(1500m));
    }}

using MediatR;using Microsoft.AspNetCore.Mvc;using Moq;using NUnit.Framework;using OrderService.WebApi.Clients;using OrderService.WebApi.Controllers;using OrderService.WebApi.DTOs;using OrderService.WebApi.UseCases.Commands;namespace OrderService.Tests;[TestFixture]public class OrdersControllerTests{
    private Mock<IMediator> _mediatorMock = null!;
    private Mock<IPaymentClient> _paymentClientMock = null!;
    private OrdersController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>();
        _paymentClientMock = new Mock<IPaymentClient>();
        _controller = new OrdersController(_mediatorMock.Object, _paymentClientMock.Object);
    }

    [Test]
    public async Task CreateOrder_ShouldReturnCreatedAtActionWithOrderAndPayment()
    {
        var createDto = new CreateOrderDto("Алексей", 2000m);
        var orderResponse = new OrderResponseDto(Guid.NewGuid(), "Алексей", 2000m, "Created", DateTime.UtcNow);
        var paymentResponse = new ProcessPaymentResponse(Guid.NewGuid(), orderResponse.Id, 2000m, "Completed", DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderResponse);

        _paymentClientMock.Setup(p => p.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
            .ReturnsAsync(paymentResponse);

        var actionResult = await _controller.CreateOrder(createDto);

        var createdResult = actionResult as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.StatusCode, Is.EqualTo(201));

        _paymentClientMock.Verify(p => p.ProcessPaymentAsync(It.Is<ProcessPaymentRequest>(
            req => req.OrderId == orderResponse.Id && req.Amount == orderResponse.TotalAmount)), Times.Once);
    }}
using Refit;namespace OrderService.WebApi.Clients;public record ProcessPaymentRequest(Guid OrderId, decimal Amount);public record ProcessPaymentResponse(Guid Id, Guid OrderId, decimal Amount, string Status, DateTime ProcessedAt);public interface IPaymentClient{
    [Post("/api/payments/process")]
    Task<ProcessPaymentResponse> ProcessPaymentAsync([Body] ProcessPaymentRequest request);}
using MediatR;using Microsoft.AspNetCore.Mvc;using OrderService.WebApi.Clients;using OrderService.WebApi.DTOs;using OrderService.WebApi.UseCases.Commands;using OrderService.WebApi.UseCases.Queries;namespace OrderService.WebApi.Controllers;[ApiController][Route("api/[controller]")]public class OrdersController : ControllerBase{
    private readonly IMediator _mediator;
    private readonly IPaymentClient _paymentClient;

    public OrdersController(IMediator mediator, IPaymentClient paymentClient)
    {
        _mediator = mediator;
        _paymentClient = paymentClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var order = await _mediator.Send(new CreateOrderCommand(dto));

        var paymentResponse = await _paymentClient.ProcessPaymentAsync(new ProcessPaymentRequest(order.Id, order.TotalAmount));

        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, new { Order = order, Payment = paymentResponse });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id));
        if (order == null) return NotFound();
        return Ok(order);
    }}
namespace OrderService.WebApi.DTOs;public record CreateOrderDto(string CustomerName, decimal TotalAmount);public record OrderResponseDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt);
using FluentValidation;using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Mvc.Filters;namespace OrderService.WebApi.Filters;public class ValidationExceptionFilter : IExceptionFilter{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var details = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Ошибка валидации данных"
            };

            context.Result = new BadRequestObjectResult(details);
            context.ExceptionHandled = true;
        }
    }}
using AutoMapper;using OrderService.DataAccess.Entities;using OrderService.WebApi.DTOs;namespace OrderService.WebApi.Mappings;public class OrderMappingProfile : Profile{
    public OrderMappingProfile()
    {
        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Created))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<Order, OrderResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }}
using FluentValidation;using MediatR;namespace OrderService.WebApi.PipelineBehaviors;public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>{
    private readonly IServiceProvider _serviceProvider;

    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var properties = request.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var propValue = prop.GetValue(request);
            if (propValue == null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(prop.PropertyType);
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                var context = new ValidationContext<object>(propValue);
                var result = await validator.ValidateAsync(context, cancellationToken);

                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }
            }
        }

        return await next();
    }}
using AutoMapper;using MediatR;using OrderService.DataAccess;using OrderService.DataAccess.Entities;using OrderService.WebApi.DTOs;namespace OrderService.WebApi.UseCases.Commands;public record CreateOrderCommand(CreateOrderDto OrderDto) : IRequest<OrderResponseDto>;public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(OrderDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = _mapper.Map<Order>(request.OrderDto);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderResponseDto>(order);
    }}
using Dapper;using MediatR;using Microsoft.EntityFrameworkCore;using OrderService.DataAccess;using OrderService.DataAccess.Entities;using OrderService.WebApi.DTOs;using System.Data;namespace OrderService.WebApi.UseCases.Queries;public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponseDto?>;public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>{
    private readonly OrderDbContext _dbContext;

    public GetOrderByIdQueryHandler(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        const string sql = @"
            SELECT 
                ""Id"", 
                ""CustomerName"", 
                ""TotalAmount"", 
                ""Status"", 
                ""CreatedAt"" 
            FROM ""Orders"" 
            WHERE ""Id"" = @OrderId";

        var orderRaw = await connection.QuerySingleOrDefaultAsync<OrderQueryResult>(sql, new { OrderId = request.OrderId });

        if (orderRaw == null) return null;

        return new OrderResponseDto(
            orderRaw.Id,
            orderRaw.CustomerName,
            orderRaw.TotalAmount,
            ((OrderStatus)orderRaw.Status).ToString(),
            orderRaw.CreatedAt
        );
    }

    private class OrderQueryResult
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }}
using FluentValidation;using OrderService.WebApi.DTOs;namespace OrderService.WebApi.Validators;public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
    }}
using FluentValidation;using MediatR;using Microsoft.EntityFrameworkCore;using OrderService.DataAccess;using OrderService.WebApi.Clients;using OrderService.WebApi.Filters;using OrderService.WebApi.PipelineBehaviors;using OrderService.WebApi.Validators;using Refit;var builder = WebApplication.CreateBuilder(args);builder.Services.AddControllers(options =>{
    options.Filters.Add<ValidationExceptionFilter>();});builder.Services.AddEndpointsApiExplorer();builder.Services.AddSwaggerGen();builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));builder.Services.AddMediatR(cfg =>{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));});builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();builder.Services.AddAutoMapper(typeof(Program).Assembly);var paymentServiceUrl = builder.Configuration["PaymentService:BaseUrl"];builder.Services.AddHttpClient("PaymentClient", c => c.BaseAddress = new Uri(paymentServiceUrl!));builder.Services.AddTransient(sp =>{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("PaymentClient");
    return RestService.For<IPaymentClient>(client);});var app = builder.Build();using (var scope = app.Services.CreateScope()){
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    dbContext.Database.Migrate();}if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();}app.UseAuthorization();app.MapControllers();app.Run();
using MediatR;using Microsoft.AspNetCore.Mvc;using PaymentService.WebApi.DTOs;using PaymentService.WebApi.UseCases.Commands;namespace PaymentService.WebApi.Controllers;[ApiController][Route("api/[controller]")]public class PaymentsController : ControllerBase{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        var result = await _mediator.Send(new ProcessPaymentCommand(dto));
        return Ok(result);
    }}

using AutoMapper;
using MediatR;
using PaymentService.DataAccess;
using PaymentService.DataAccess.Entities;
using PaymentService.WebApi.DTOs;
using PaymentService.WebApi.Services;

namespace PaymentService.WebApi.UseCases.Commands;

public record ProcessPaymentCommand(ProcessPaymentDto PaymentDto) : IRequest<PaymentResponseDto>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponseDto>
{
    private readonly PaymentDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IKafkaProducerService _kafkaProducer;

    public ProcessPaymentCommandHandler(PaymentDbContext dbContext, IMapper mapper, IKafkaProducerService kafkaProducer)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<PaymentResponseDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = _mapper.Map<Payment>(request.PaymentDto);

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _kafkaProducer.PublishPaymentCompletedAsync(payment.OrderId, payment.Id, payment.Amount);

        return _mapper.Map<PaymentResponseDto>(payment);
    }
}

using Confluent.Kafka;
using System.Text.Json;

namespace PaymentService.WebApi.Services;

public interface IKafkaProducerService
{
    Task PublishPaymentCompletedAsync(Guid orderId, Guid paymentId, decimal amount);
}

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        _topic = configuration["Kafka:Topic"] ?? "payment-events";

        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.All,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000,
            EnableDeliveryReports = true,
            ClientId = "PaymentService-Producer"
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) => _logger.LogError("Kafka Producer Error: {Reason}", error.Reason))
            .Build();
    }

    public async Task PublishPaymentCompletedAsync(Guid orderId, Guid paymentId, decimal amount)
    {
        var eventMessage = new PaymentCompletedEvent(orderId, paymentId, amount, "Completed", DateTime.UtcNow);
        var jsonPayload = JsonSerializer.Serialize(eventMessage);

        try
        {
            var result = await _producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key = orderId.ToString(),
                Value = jsonPayload
            });

            _logger.LogInformation("Event published to Kafka topic {Topic}, partition {Partition}, offset {Offset}",
                result.Topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to deliver event to Kafka: {Reason}", ex.Error.Reason);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}

public record PaymentCompletedEvent(Guid OrderId, Guid PaymentId, decimal Amount, string Status, DateTime Timestamp);

using FluentValidation;
using PaymentService.WebApi.DTOs;

namespace PaymentService.WebApi.Validators;

public class ProcessPaymentDtoValidator : AbstractValidator<ProcessPaymentDto>
{
    public ProcessPaymentDtoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess;
using PaymentService.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.DataAccess.Entities;

namespace PaymentService.DataAccess.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .HasConversion<int>();
    }
}


namespace PaymentService.DataAccess.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2
}

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime ProcessedAt { get; set; }
}


using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Entities;
using System.Reflection.Emit;

namespace PaymentService.DataAccess;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
