# Lab04 - RabbitMQ E-Commerce Microservices

## Cấu trúc Solution (9 projects)

```
Lab04_RabbitMQ.sln
│
├── 📦 Messaging.Common              [Class Library]
│   ├── Connection/ConnectionManager.cs        ← [CLASS] Quản lý RabbitMQ connection
│   ├── Consuming/BaseConsumer.cs              ← [ABSTRACT CLASS] Base consumer ACK/NACK
│   ├── Events/OrderPlacedEvent.cs             ← [CLASS] Shared event data
│   ├── Extensions/ServiceCollectionExtensions.cs ← [STATIC CLASS] AddRabbitMq() DI helper
│   ├── Models/EventBase.cs                    ← [ABSTRACT CLASS] EventId, Timestamp, CorrelationId
│   ├── Options/RabbitMqOptions.cs             ← [CLASS] POCO mapping appsettings
│   ├── Publishing/Publisher.cs                ← [CLASS] Generic publisher
│   └── Topology/RabbitTopology.cs             ← [STATIC CLASS] Declare exchange/queue/binding
│
├── 🌐 OrderServices                 [ASP.NET Core Web API - PORT 5001]
│   ├── OrderService.Application/
│   │   └── OrderService.cs                    ← [CLASS] Business logic + publish event
│   ├── OrderService.Controllers/
│   │   └── OrdersController.cs                ← [API CONTROLLER]
│   ├── OrderService.Infrastructure/Messaging/
│   │   └── RabbitMqOrderEventPublisher.cs     ← [CLASS] Implements IOrderEventPublisher
│   ├── appsettings.json
│   └── Program.cs
│
├── 📦 OrderServices.Contracts       [Class Library]
│   └── Messaging/
│       └── IOrderEventPublisher.cs            ← [INTERFACE]
│
├── 🌐 PaymentService                [ASP.NET Core Web API - PORT 5002]
│   ├── PaymentService.Application/Messaging/
│   │   └── PaymentPlacedHandler.cs            ← [CLASS] Implements IPayment
│   ├── PaymentService.Controllers/
│   │   └── PaymentController.cs               ← [API CONTROLLER]
│   ├── PaymentService.Infrastructure/Messaging/
│   │   └── OrderPlacedConsumer.cs             ← [CLASS] BackgroundService consumer
│   ├── appsettings.json
│   └── Program.cs
│
├── 📦 PaymentServices.Contract      [Class Library]
│   └── Messaging/
│       └── IPayment.cs                        ← [INTERFACE]
│
├── 🌐 ProductService                [ASP.NET Core Web API - PORT 5003]
│   ├── ProductService.Application/Messaging/
│   │   └── OrderPlacedHandler.cs              ← [CLASS] Implements IOrderPlacedHandler → giảm stock
│   ├── ProductService.Controllers/
│   │   └── ProductsController.cs              ← [API CONTROLLER]
│   ├── ProductService.Infrastructure/Messaging/
│   │   └── OrderPlacedConsumer.cs             ← [CLASS] BackgroundService consumer
│   ├── appsettings.json
│   └── Program.cs
│
├── 📦 ProductServices.Contracts     [Class Library]
│   └── Messaging/
│       └── IOrderPlacedHandler.cs             ← [INTERFACE]
│
├── 🌐 NotificationService           [ASP.NET Core Web API - PORT 5004]
│   ├── NotificationService.Application/Messaging/
│   │   └── OrderPlacedHandler.cs              ← [CLASS] Implements IOrderPlacedHandler → tạo notification
│   ├── NotificationService.Controllers/
│   │   └── NotificationsController.cs         ← [API CONTROLLER]
│   ├── NotificationService.Infrastructure/Messaging/
│   │   └── OrderPlacedConsumer.cs             ← [CLASS] BackgroundService consumer
│   ├── appsettings.json
│   └── Program.cs
│
└── 📦 NotificationServices.Contracts [Class Library]
    └── Messaging/
        └── IOrderPlacedHandler.cs             ← [INTERFACE]
```

## Quan hệ giữa các layers

```
OrderServices ──publish──► RabbitMQ Exchange "ecommerce.topic"
                               │
                    routing key "order.placed"
                               │
              ┌────────────────┴────────────────┐
              ▼                                  ▼
   queue: product.order_placed      queue: notification.order_placed
              │                                  │
              ▼                                  ▼
      ProductService                   NotificationService
   (giảm stock sản phẩm)           (tạo email notification)

PaymentService cũng consume từ product.order_placed (demo)
```

## NuGet Packages cần cài

| Project | Package | Version |
|---------|---------|---------|
| Messaging.Common | RabbitMQ.Client | 6.5.0 |
| Messaging.Common | Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.0 |
| OrderServices | Swashbuckle.AspNetCore | 6.5.0 |
| OrderServices | Microsoft.Extensions.Options.ConfigurationExtensions | 8.0.0 |
| PaymentService | RabbitMQ.Client | 6.5.0 |
| PaymentService | Swashbuckle.AspNetCore | 6.5.0 |
| PaymentService | Microsoft.Extensions.Options.ConfigurationExtensions | 8.0.0 |
| ProductService | RabbitMQ.Client | 6.5.0 |
| ProductService | Swashbuckle.AspNetCore | 6.5.0 |
| ProductService | Microsoft.Extensions.Options.ConfigurationExtensions | 8.0.0 |
| NotificationService | RabbitMQ.Client | 6.5.0 |
| NotificationService | Swashbuckle.AspNetCore | 6.5.0 |
| NotificationService | Microsoft.Extensions.Options.ConfigurationExtensions | 8.0.0 |

## Bước Setup RabbitMQ

### 1. Cài RabbitMQ (Windows)
- Download: https://www.rabbitmq.com/download.html
- Enable Management Plugin:
```bash
rabbitmq-plugins enable rabbitmq_management
```
- Management UI: http://localhost:15672 (guest/guest)

### 2. Tạo Virtual Host & User
```bash
# Tạo virtual host
rabbitmqctl add_vhost ecommerce_vhost

# Tạo user
rabbitmqctl add_user ecommerce_user Test@1234

# Gán quyền
rabbitmqctl set_permissions -p ecommerce_vhost ecommerce_user ".*" ".*" ".*"
```

## Cách chạy

### Thứ tự khởi động (QUAN TRỌNG)
Consumer phải khởi động TRƯỚC Publisher:

```
1. ProductService      (port 5003) - consumer sẵn sàng
2. NotificationService (port 5004) - consumer sẵn sàng  
3. PaymentService      (port 5002) - consumer sẵn sàng
4. OrderServices       (port 5001) - publisher
```

### Test Flow

**1. Tạo đơn hàng COD (tự publish ngay):**
```http
POST http://localhost:5001/api/orders
Content-Type: application/json

{
  "userId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "customerName": "Nguyễn Văn A",
  "customerEmail": "nguyenvana@gmail.com",
  "phoneNumber": "0901234567",
  "paymentMethod": "COD",
  "items": [
    {
      "productId": "11111111-1111-1111-1111-111111111111",
      "productName": "Laptop Dell XPS 15",
      "quantity": 1,
      "unitPrice": 25000000
    },
    {
      "productId": "22222222-2222-2222-2222-222222222222",
      "productName": "iPhone 15 Pro Max",
      "quantity": 2,
      "unitPrice": 32000000
    }
  ]
}
```

**2. Kiểm tra stock đã giảm:**
```http
GET http://localhost:5003/api/products
```

**3. Kiểm tra notification đã tạo:**
```http
GET http://localhost:5004/api/notifications
```

**4. Tạo đơn hàng Online Payment (confirm sau):**
```http
POST http://localhost:5001/api/orders
{ ... "paymentMethod": "CreditCard" ... }

# Sau đó confirm:
POST http://localhost:5001/api/orders/{orderId}/confirm
```

## Kiến trúc RabbitMQ

```
Exchange: ecommerce.topic (topic)
    │
    ├── routing: order.placed ──► queue: product.order_placed
    │                          └► queue: notification.order_placed
    │
DLX: ecommerce.dlx (fanout)
    └──► queue: ecommerce.dlq (Dead Letter Queue)
```
