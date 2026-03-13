using Messaging.Common.Extensions;
using Messaging.Common.Options;
using PaymentService.PaymentService.Application.Messaging;
using PaymentService.PaymentService.Infrastructure.Messaging;
using PaymentServices.Contract.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
var mq = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>()!;
builder.Services.AddRabbitMq(mq.HostName, mq.UserName, mq.Password, mq.VirtualHost);
builder.Services.AddScoped<IPayment, PaymentPlacedHandler>();
builder.Services.AddHostedService<OrderPlacedConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "PaymentService API", Version = "v1" }));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
