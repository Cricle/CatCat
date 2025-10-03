# CatCat Stripe 支付集成指南

> 基于 Stripe.net SDK 的深度集成实现
> 更新时间: 2025-10-03

---

## 📋 目录

1. [概述](#概述)
2. [功能特性](#功能特性)
3. [快速开始](#快速开始)
4. [配置说明](#配置说明)
5. [支付流程](#支付流程)
6. [Webhook 集成](#webhook-集成)
7. [API 参考](#api-参考)
8. [最佳实践](#最佳实践)
9. [故障排查](#故障排查)

---

## 概述

CatCat 使用 **Stripe.net SDK** 提供完整的支付功能，包括：
- Payment Intent（支付意图）
- 退款管理
- 客户管理
- 支付方法管理
- Webhook 事件处理
- 分布式追踪集成

---

## 功能特性

### ✅ 已实现功能

#### 1. **Payment Intent（支付意图）**
- ✅ 创建支付意图
- ✅ 获取支付状态
- ✅ 确认支付
- ✅ 取消支付
- ✅ 自动支付方法
- ✅ 元数据支持（订单ID等）

#### 2. **退款管理**
- ✅ 全额退款
- ✅ 部分退款
- ✅ 退款原因标记
- ✅ 退款状态查询

#### 3. **客户管理**
- ✅ 创建 Stripe 客户
- ✅ 获取客户信息
- ✅ 更新客户资料
- ✅ 删除客户
- ✅ 元数据关联（CatCat 用户ID）

#### 4. **支付方法管理**
- ✅ 附加支付方法到客户
- ✅ 移除支付方法
- ✅ 列出客户的支付方法
- ✅ 支持卡片支付

#### 5. **Webhook 事件处理**
- ✅ 签名验证
- ✅ 支付成功事件
- ✅ 支付失败事件
- ✅ 支付取消事件
- ✅ 退款事件
- ✅ 客户事件
- ✅ 支付方法事件

#### 6. **分布式追踪**
- ✅ 所有 Stripe API 调用都有追踪
- ✅ 记录支付金额、货币、状态
- ✅ 异常记录和错误追踪
- ✅ Jaeger 可视化

---

## 快速开始

### 1. 获取 Stripe API 密钥

1. 注册 Stripe 账号: https://dashboard.stripe.com/register
2. 切换到**测试模式**（右上角开关）
3. 获取密钥:
   - **秘密密钥**: `sk_test_...`（用于服务端）
   - **可发布密钥**: `pk_test_...`（用于客户端）

### 2. 配置 API

编辑 `src/CatCat.API/appsettings.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_51MzXXXXXXXXXX...",
    "PublishableKey": "pk_test_51MzXXXXXXXXXX...",
    "WebhookSecret": "whsec_XXXXXXXXXX"
  }
}
```

> ⚠️ **生产环境**: 使用环境变量或密钥管理服务存储密钥

### 3. 启动应用

```bash
cd src/CatCat.API
dotnet run
```

### 4. 测试支付

使用 Stripe 测试卡号:
- **成功**: `4242 4242 4242 4242`
- **失败**: `4000 0000 0000 0002`
- **需要验证**: `4000 0025 0000 3155`

---

## 配置说明

### 环境变量配置

#### Docker Compose

```yaml
services:
  api:
    environment:
      Stripe__SecretKey: "sk_test_..."
      Stripe__PublishableKey: "pk_test_..."
      Stripe__WebhookSecret: "whsec_..."
```

#### Kubernetes

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: stripe-secrets
type: Opaque
stringData:
  secret-key: "sk_test_..."
  webhook-secret: "whsec_..."
```

---

## 支付流程

### 标准支付流程

```
1. 用户创建订单
   ↓
2. 后端创建 Payment Intent
   POST /api/orders
   ↓
3. 返回 client_secret 给前端
   ↓
4. 前端使用 Stripe.js 确认支付
   ↓
5. Stripe 处理支付
   ↓
6. Webhook 通知支付结果
   POST /api/webhooks/stripe
   ↓
7. 后端更新订单状态
```

### 代码示例

#### 1. 创建订单和支付意图

```csharp
// OrderService.cs
public async Task<Result<long>> CreateOrderAsync(CreateOrderCommand command)
{
    // 1. 创建订单
    var order = new ServiceOrder { ... };
    var orderId = await _orderRepository.CreateAsync(order);

    // 2. 创建支付意图
    var paymentResult = await _paymentService.CreatePaymentIntentAsync(
        orderId,
        order.TotalAmount,
        "cny");

    if (!paymentResult.Success)
        return Result.Failure<long>(paymentResult.ErrorMessage!);

    // 3. 保存支付记录
    var payment = new Payment
    {
        OrderId = orderId,
        PaymentIntentId = paymentResult.PaymentIntentId,
        Amount = order.TotalAmount,
        Currency = "cny",
        Status = "pending"
    };
    await _paymentRepository.CreateAsync(payment);

    return Result.Success(orderId);
}
```

#### 2. 前端确认支付 (Vue 3 示例)

```typescript
// Frontend: createOrder.ts
import { loadStripe } from '@stripe/stripe-js';

const stripe = await loadStripe('pk_test_...');

// 创建订单
const response = await orderApi.create({
  servicePackageId: 1,
  petId: 2,
  serviceDate: '2025-10-10',
  // ...
});

// 确认支付
const { error } = await stripe!.confirmCardPayment(response.data.clientSecret, {
  payment_method: {
    card: cardElement,
    billing_details: {
      name: 'Customer Name',
    },
  },
});

if (error) {
  console.error('Payment failed:', error.message);
} else {
  console.log('Payment successful!');
}
```

#### 3. Webhook 处理支付成功

```csharp
// StripeWebhookEndpoints.cs
private static async Task HandlePaymentIntentSucceeded(
    Event stripeEvent,
    IOrderService orderService)
{
    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
    var orderId = long.Parse(paymentIntent.Metadata["order_id"]);

    // 更新订单状态为已支付
    await orderService.PayOrderAsync(orderId, paymentIntent.Id);
}
```

---

## Webhook 集成

### 1. 配置 Webhook 端点

**本地开发**: 使用 Stripe CLI

```bash
# 安装 Stripe CLI
# macOS
brew install stripe/stripe-cli/stripe

# Windows
scoop bucket add stripe https://github.com/stripe/scoop-stripe-cli.git
scoop install stripe

# 登录
stripe login

# 转发 Webhook 到本地
stripe listen --forward-to localhost:8080/api/webhooks/stripe
```

Stripe CLI 会输出 webhook secret:
```
> Ready! Your webhook signing secret is whsec_xxxxx
```

将这个 secret 添加到配置:
```json
{
  "Stripe": {
    "WebhookSecret": "whsec_xxxxx"
  }
}
```

### 2. 生产环境配置

1. 登录 Stripe Dashboard
2. **开发者** → **Webhooks** → **添加端点**
3. 端点 URL: `https://api.catcat.com/api/webhooks/stripe`
4. 选择事件:
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
   - `payment_intent.canceled`
   - `charge.refunded`
   - `charge.refund.updated`
5. 保存，获取 **签名密钥**

### 3. 支持的 Webhook 事件

| 事件类型 | 说明 | 处理方式 |
|---------|------|---------|
| `payment_intent.succeeded` | 支付成功 | 更新订单状态为已支付 |
| `payment_intent.payment_failed` | 支付失败 | 记录日志，通知用户 |
| `payment_intent.canceled` | 支付取消 | 更新订单状态为已取消 |
| `charge.refunded` | 退款完成 | 更新订单退款状态 |
| `charge.refund.updated` | 退款更新 | 记录退款状态变化 |
| `payment_method.attached` | 支付方法附加 | 记录日志 |
| `customer.created` | 客户创建 | 记录日志 |
| `customer.deleted` | 客户删除 | 记录日志 |

---

## API 参考

### Payment Intent

#### 创建支付意图

```csharp
var result = await paymentService.CreatePaymentIntentAsync(
    orderId: 12345,
    amount: 99.00m,
    currency: "cny"
);

// result.Success
// result.PaymentIntentId
// result.ClientSecret  // 发送给前端
```

#### 确认支付

```csharp
bool succeeded = await paymentService.ConfirmPaymentAsync("pi_xxx");
```

#### 取消支付

```csharp
bool canceled = await paymentService.CancelPaymentAsync("pi_xxx");
```

### 退款

#### 全额退款

```csharp
var refund = await paymentService.RefundPaymentAsync(
    paymentIntentId: "pi_xxx"
);
```

#### 部分退款

```csharp
var refund = await paymentService.RefundPaymentAsync(
    paymentIntentId: "pi_xxx",
    amount: 50.00m,
    reason: "requested_by_customer"
);
```

### 客户管理

#### 创建客户

```csharp
var customer = await paymentService.CreateCustomerAsync(
    userId: 123,
    email: "user@example.com",
    name: "张三",
    metadata: new Dictionary<string, string>
    {
        { "user_id", "123" },
        { "platform", "catcat" }
    }
);
```

#### 更新客户

```csharp
var customer = await paymentService.UpdateCustomerAsync(
    customerId: "cus_xxx",
    email: "newemail@example.com",
    name: "李四"
);
```

### 支付方法

#### 附加支付方法

```csharp
var paymentMethod = await paymentService.AttachPaymentMethodAsync(
    paymentMethodId: "pm_xxx",
    customerId: "cus_xxx"
);
```

#### 列出支付方法

```csharp
var methods = await paymentService.ListCustomerPaymentMethodsAsync("cus_xxx");
```

---

## 最佳实践

### 1. **幂等性**

每个支付请求都应该有唯一的幂等性密钥:

```csharp
var options = new PaymentIntentCreateOptions
{
    Amount = 10000,
    Currency = "cny",
    // 幂等性密钥（防止重复支付）
    Metadata = new Dictionary<string, string>
    {
        { "order_id", orderId.ToString() },
        { "idempotency_key", Guid.NewGuid().ToString() }
    }
};
```

### 2. **错误处理**

捕获并记录所有 Stripe 异常:

```csharp
try
{
    var paymentIntent = await _paymentIntentService.CreateAsync(options);
}
catch (StripeException ex)
{
    _logger.LogError(ex, "Stripe API error: {ErrorCode} - {ErrorMessage}",
        ex.StripeError?.Code, ex.Message);

    // 返回用户友好的错误消息
    return new PaymentIntentResult
    {
        Success = false,
        ErrorMessage = GetUserFriendlyErrorMessage(ex.StripeError?.Code)
    };
}
```

### 3. **Webhook 安全**

始终验证 Webhook 签名:

```csharp
var stripeEvent = EventUtility.ConstructEvent(
    json,
    request.Headers["Stripe-Signature"],
    webhookSecret,
    throwOnApiVersionMismatch: false
);
```

### 4. **异步处理**

Webhook 应该快速响应，耗时操作放到后台:

```csharp
private static async Task<IResult> HandleStripeWebhook(...)
{
    // 快速验证和响应
    var stripeEvent = EventUtility.ConstructEvent(...);

    // 后台处理
    _ = Task.Run(() => ProcessEventAsync(stripeEvent));

    // 立即返回 200
    return Results.Ok(new { received = true });
}
```

### 5. **元数据使用**

在所有 Stripe 对象中添加元数据以便关联:

```csharp
Metadata = new Dictionary<string, string>
{
    { "order_id", orderId.ToString() },
    { "user_id", userId.ToString() },
    { "source", "catcat-api" },
    { "created_at", DateTime.UtcNow.ToString("O") }
}
```

---

## 故障排查

### 问题 1: Webhook 签名验证失败

**症状**: `Invalid Stripe webhook signature` 错误

**解决方案**:
1. 检查 `Stripe:WebhookSecret` 配置是否正确
2. 确认使用的是正确环境的密钥（测试/生产）
3. 本地开发使用 `stripe listen` 获取 webhook secret

### 问题 2: 支付意图创建失败

**症状**: `StripeException: Invalid API Key`

**解决方案**:
1. 检查 `Stripe:SecretKey` 配置
2. 确认密钥以 `sk_test_` 或 `sk_live_` 开头
3. 确认没有多余的空格或换行符

### 问题 3: 测试卡支付失败

**症状**: 支付总是失败

**解决方案**:
1. 使用 Stripe 测试卡: `4242 4242 4242 4242`
2. 过期日期使用未来日期
3. CVC 使用任意 3 位数字
4. 邮编使用任意 5 位数字

### 问题 4: Webhook 未触发

**症状**: 支付成功但订单状态未更新

**解决方案**:
1. 检查 Stripe Dashboard → Webhooks → 日志
2. 确认端点 URL 正确且可访问
3. 检查选择的事件类型
4. 本地开发使用 `stripe listen --forward-to`

---

## 测试

### 测试卡号

| 卡号 | 说明 |
|------|------|
| `4242 4242 4242 4242` | 成功支付 |
| `4000 0000 0000 0002` | 卡被拒绝 |
| `4000 0025 0000 3155` | 需要 3D Secure 验证 |
| `4000 0000 0000 9995` | 余额不足 |

### Webhook 测试

```bash
# 使用 Stripe CLI 发送测试事件
stripe trigger payment_intent.succeeded

# 查看 webhook 日志
stripe logs tail
```

---

## 监控

### Grafana Dashboard

所有 Stripe API 调用都有追踪和指标:

1. **访问 Grafana**: http://localhost:3001
2. **查看仪表板**: CatCat Business Metrics
3. **查看指标**:
   - 支付成功率
   - 支付金额统计
   - 退款统计
   - API 调用延迟

### Jaeger 追踪

查看支付流程的完整追踪:

1. **访问 Jaeger**: http://localhost:16686
2. **搜索服务**: `CatCat.API`
3. **筛选操作**: `Stripe.CreatePaymentIntent`
4. **查看详情**:
   - 支付金额
   - 订单ID
   - 支付状态
   - 错误信息

---

## 参考资源

- [Stripe.net 官方文档](https://github.com/stripe/stripe-dotnet)
- [Stripe API 文档](https://stripe.com/docs/api)
- [Stripe Webhook 文档](https://stripe.com/docs/webhooks)
- [Stripe 测试文档](https://stripe.com/docs/testing)
- [Stripe Dashboard](https://dashboard.stripe.com/)

---

## 许可证

本项目使用 MIT 许可证。Stripe.net SDK 使用 Apache 2.0 许可证。

