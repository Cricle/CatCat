# CatCat API 文档

## 基础信息

- **Base URL**: `http://localhost:5000/api`
- **Content-Type**: `application/json`
- **认证方式**: Bearer Token (JWT)

## 认证相关 API

### 1. 发送验证码

发送短信验证码到指定手机号。

**请求**
```http
POST /api/auth/send-code
Content-Type: application/json

{
  "phone": "13800138000"
}
```

**响应**
```json
{
  "message": "验证码已发送"
}
```

---

### 2. 用户注册

使用手机号和验证码注册新用户。

**请求**
```http
POST /api/auth/register
Content-Type: application/json

{
  "phone": "13800138000",
  "code": "123456",
  "password": "password123",
  "nickName": "小明"
}
```

**响应**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "phone": "13800138000",
    "nickName": "小明",
    "avatar": null,
    "role": 1
  }
}
```

---

### 3. 用户登录

使用手机号和密码登录。

**请求**
```http
POST /api/auth/login
Content-Type: application/json

{
  "phone": "13800138000",
  "password": "password123"
}
```

**响应**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "phone": "13800138000",
    "nickName": "小明",
    "avatar": "https://example.com/avatar.jpg",
    "role": 1
  }
}
```

---

## 用户相关 API

### 4. 获取当前用户信息

**请求**
```http
GET /api/users/me
Authorization: Bearer {token}
```

**响应**
```json
{
  "id": 1,
  "phone": "13800138000",
  "email": "user@example.com",
  "nickName": "小明",
  "avatar": "https://example.com/avatar.jpg",
  "role": 1,
  "status": 1,
  "createdAt": "2025-01-01T00:00:00Z"
}
```

---

### 5. 更新用户信息

**请求**
```http
PUT /api/users/me
Authorization: Bearer {token}
Content-Type: application/json

{
  "nickName": "小明",
  "email": "newemail@example.com",
  "avatar": "https://example.com/new-avatar.jpg"
}
```

**响应**
```json
{
  "message": "更新成功"
}
```

---

## 宠物相关 API

### 6. 获取我的宠物列表

**请求**
```http
GET /api/pets
Authorization: Bearer {token}
```

**响应**
```json
{
  "items": [
    {
      "id": 1,
      "name": "咪咪",
      "type": 1,
      "breed": "英短",
      "age": 2,
      "gender": 2,
      "avatar": "https://example.com/cat.jpg",
      "character": "温顺粘人",
      "dietaryHabits": "猫粮+罐头",
      "healthStatus": "健康",
      "remarks": "怕生人"
    }
  ],
  "total": 1
}
```

---

### 7. 添加宠物

**请求**
```http
POST /api/pets
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "咪咪",
  "type": 1,
  "breed": "英短",
  "age": 2,
  "gender": 2,
  "avatar": "https://example.com/cat.jpg",
  "character": "温顺粘人",
  "dietaryHabits": "猫粮+罐头",
  "healthStatus": "健康",
  "remarks": "怕生人"
}
```

**响应**
```json
{
  "id": 1,
  "message": "添加成功"
}
```

---

## 服务套餐 API

### 8. 获取服务套餐列表

**请求**
```http
GET /api/service-packages
```

**响应**
```json
{
  "items": [
    {
      "id": 1,
      "name": "基础喂养",
      "description": "上门喂食、换水、清理猫砂",
      "price": 30.00,
      "duration": 30,
      "iconUrl": null,
      "serviceItems": "[\"喂食\",\"换水\",\"清理猫砂\"]",
      "isActive": true
    },
    {
      "id": 2,
      "name": "标准护理",
      "description": "基础喂养 + 梳毛 + 陪玩",
      "price": 50.00,
      "duration": 60,
      "iconUrl": null,
      "serviceItems": "[\"喂食\",\"换水\",\"清理猫砂\",\"梳毛\",\"陪玩15分钟\"]",
      "isActive": true
    }
  ],
  "total": 2
}
```

---

## 订单相关 API

### 9. 创建订单

**请求**
```http
POST /api/orders
Authorization: Bearer {token}
Content-Type: application/json

{
  "petId": 1,
  "servicePackageId": 2,
  "serviceDate": "2025-10-10",
  "serviceTime": "14:00:00",
  "address": "北京市朝阳区xxx小区",
  "addressDetail": "1号楼101室",
  "customerRemark": "猫咪比较胆小，请轻声说话"
}
```

**响应**
```json
{
  "orderId": 1,
  "orderNo": "202510100001",
  "message": "订单创建成功"
}
```

---

### 10. 获取我的订单列表

**请求**
```http
GET /api/orders?status=0&page=1&pageSize=10
Authorization: Bearer {token}
```

**参数**
- `status` (可选): 订单状态 (0=待接单, 1=已接单, 2=服务中, 3=已完成, 4=已取消)
- `page`: 页码，从1开始
- `pageSize`: 每页数量

**响应**
```json
{
  "items": [
    {
      "id": 1,
      "orderNo": "202510100001",
      "customerId": 1,
      "serviceProviderId": null,
      "petId": 1,
      "servicePackageId": 2,
      "serviceDate": "2025-10-10T00:00:00Z",
      "serviceTime": "14:00:00",
      "address": "北京市朝阳区xxx小区",
      "addressDetail": "1号楼101室",
      "price": 50.00,
      "status": 0,
      "customerRemark": "猫咪比较胆小，请轻声说话",
      "createdAt": "2025-10-02T10:00:00Z"
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 10
}
```

---

### 11. 获取订单详情

**请求**
```http
GET /api/orders/{id}
Authorization: Bearer {token}
```

**响应**
```json
{
  "id": 1,
  "orderNo": "202510100001",
  "customer": {
    "id": 1,
    "nickName": "小明",
    "phone": "13800138000",
    "avatar": "https://example.com/avatar.jpg"
  },
  "serviceProvider": null,
  "pet": {
    "id": 1,
    "name": "咪咪",
    "type": 1,
    "avatar": "https://example.com/cat.jpg"
  },
  "servicePackage": {
    "id": 2,
    "name": "标准护理",
    "description": "基础喂养 + 梳毛 + 陪玩",
    "price": 50.00
  },
  "serviceDate": "2025-10-10T00:00:00Z",
  "serviceTime": "14:00:00",
  "address": "北京市朝阳区xxx小区",
  "addressDetail": "1号楼101室",
  "price": 50.00,
  "status": 0,
  "customerRemark": "猫咪比较胆小，请轻声说话",
  "serviceRecords": [],
  "createdAt": "2025-10-02T10:00:00Z"
}
```

---

### 12. 取消订单

**请求**
```http
POST /api/orders/{id}/cancel
Authorization: Bearer {token}
Content-Type: application/json

{
  "reason": "临时有事，无法继续服务"
}
```

**响应**
```json
{
  "message": "订单已取消"
}
```

---

## 服务人员相关 API

### 13. 申请成为服务人员

**请求**
```http
POST /api/service-providers/apply
Authorization: Bearer {token}
Content-Type: application/json

{
  "realName": "张三",
  "idCard": "110101199001011234",
  "idCardFrontUrl": "https://example.com/id-front.jpg",
  "idCardBackUrl": "https://example.com/id-back.jpg",
  "certificateUrl": "https://example.com/cert.jpg",
  "introduction": "5年养猫经验，持有宠物护理证书"
}
```

**响应**
```json
{
  "id": 1,
  "message": "申请已提交，等待审核"
}
```

---

### 14. 接单

**请求**
```http
POST /api/orders/{id}/accept
Authorization: Bearer {token}
```

**响应**
```json
{
  "message": "接单成功"
}
```

---

### 15. 开始服务

**请求**
```http
POST /api/orders/{id}/start
Authorization: Bearer {token}
```

**响应**
```json
{
  "message": "服务已开始"
}
```

---

### 16. 上传服务记录

**请求**
```http
POST /api/orders/{id}/records
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: (binary)
type: 1  // 1=照片, 2=视频
description: "正在喂食"
```

**响应**
```json
{
  "id": 1,
  "fileUrl": "https://example.com/record.jpg",
  "thumbnailUrl": "https://example.com/thumb.jpg",
  "message": "上传成功"
}
```

---

### 17. 完成服务

**请求**
```http
POST /api/orders/{id}/complete
Authorization: Bearer {token}
Content-Type: application/json

{
  "serviceRemark": "服务已完成，猫咪状态良好"
}
```

**响应**
```json
{
  "message": "服务已完成"
}
```

---

## 评价相关 API

### 18. 评价订单

**请求**
```http
POST /api/reviews
Authorization: Bearer {token}
Content-Type: application/json

{
  "orderId": 1,
  "rating": 5,
  "content": "服务非常好，下次还会预约",
  "photoUrls": "[\"https://example.com/photo1.jpg\"]"
}
```

**响应**
```json
{
  "id": 1,
  "message": "评价成功"
}
```

---

### 19. 获取服务人员评价列表

**请求**
```http
GET /api/service-providers/{id}/reviews?page=1&pageSize=10
```

**响应**
```json
{
  "items": [
    {
      "id": 1,
      "orderId": 1,
      "customer": {
        "nickName": "小明",
        "avatar": "https://example.com/avatar.jpg"
      },
      "rating": 5,
      "content": "服务非常好，下次还会预约",
      "photoUrls": "[\"https://example.com/photo1.jpg\"]",
      "createdAt": "2025-10-10T20:00:00Z"
    }
  ],
  "total": 1,
  "averageRating": 5.0
}
```

---

## 错误响应格式

所有API在出错时返回统一格式：

```json
{
  "code": 400,
  "message": "错误描述",
  "detail": "详细错误信息"
}
```

常见状态码：
- `200` - 成功
- `400` - 请求参数错误
- `401` - 未认证
- `403` - 无权限
- `404` - 资源不存在
- `429` - 请求频率限制
- `500` - 服务器错误

---

## 在线文档

更多API详情请访问 Swagger UI: http://localhost:5000/swagger

