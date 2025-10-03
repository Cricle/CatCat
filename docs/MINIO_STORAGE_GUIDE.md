# MinIO 对象存储集成指南

> CatCat 项目使用 MinIO 存储图片和视频等媒体文件
> 更新时间: 2025-10-03

---

## 📋 概述

**MinIO** 是一个高性能、S3 兼容的对象存储服务，用于存储用户上传的宠物照片、服务进度照片、视频等媒体文件。

### 为什么选择 MinIO？

- ✅ **S3 兼容**: 完全兼容 Amazon S3 API
- ✅ **高性能**: 专为高吞吐量设计
- ✅ **开源免费**: MIT 协议，可商用
- ✅ **易于部署**: Docker 单容器部署
- ✅ **集群支持**: 支持分布式部署
- ✅ **Web 控制台**: 内置管理界面

---

## 🏗️ 架构设计

### 文件存储流程

```
客户端 → API (Upload) → MinIO → 返回文件标识
                         ↓
                    持久化存储
                         ↓
      客户端 ← 预签名URL (7天有效) ← GetFileUrl API
```

### 关键特性

1. **唯一文件名**: 使用 `Guid` 生成唯一文件名，避免冲突
2. **预签名 URL**: 生成临时访问 URL（7天有效），无需暴露密钥
3. **文件验证**: 
   - 最大 50MB
   - 仅支持图片和视频格式
4. **自动初始化**: 应用启动时自动创建 Bucket

---

## 🐳 Docker 部署

### docker-compose.yml 配置

```yaml
minio:
  image: minio/minio:latest
  container_name: catcat-minio
  command: server /data --console-address ":9001"
  environment:
    MINIO_ROOT_USER: catcat
    MINIO_ROOT_PASSWORD: catcat_minio_password_change_in_production
  ports:
    - "9000:9000"  # API
    - "9001:9001"  # Web Console
  volumes:
    - minio_data:/data
  healthcheck:
    test: ["CMD", "mc", "ready", "local"]
    interval: 10s
    timeout: 5s
    retries: 5
  networks:
    - catcat-network
```

### 访问地址

- **API 端点**: http://localhost:9000
- **Web 控制台**: http://localhost:9001
  - 用户名: `catcat`
  - 密码: `catcat_minio_password_change_in_production`

---

## ⚙️ 配置

### appsettings.json

```json
{
  "MinIO": {
    "Endpoint": "localhost:9000",
    "AccessKey": "catcat",
    "SecretKey": "catcat_minio_password",
    "BucketName": "catcat-media",
    "UseSSL": false
  }
}
```

### 环境变量

```bash
MinIO__Endpoint=minio:9000
MinIO__AccessKey=catcat
MinIO__SecretKey=catcat_minio_password
MinIO__BucketName=catcat-media
MinIO__UseSSL=false
```

---

## 📡 API 端点

### 1. 上传文件

**POST** `/api/storage/upload`

**请求**:
- Content-Type: `multipart/form-data`
- 需要认证: ✅

**表单字段**:
- `file`: 文件数据（最大 50MB）

**支持的文件类型**:
- 图片: `image/jpeg`, `image/png`, `image/gif`, `image/webp`
- 视频: `video/mp4`, `video/mpeg`, `video/quicktime`, `video/x-msvideo`

**响应**:
```json
{
  "fileName": "3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg",
  "url": "http://localhost:9000/catcat-media/3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg?...",
  "contentType": "image/jpeg",
  "size": 1024000
}
```

### 2. 删除文件

**DELETE** `/api/storage/{fileName}`

**请求**:
- 需要认证: ✅

**响应**:
```json
{
  "message": "File deleted successfully"
}
```

### 3. 获取文件URL

**GET** `/api/storage/{fileName}/url`

**请求**:
- 需要认证: ✅

**响应**:
```json
{
  "url": "http://localhost:9000/catcat-media/3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg?..."
}
```

---

## 🔧 代码示例

### 前端上传文件

```typescript
// uploadFile.ts
import axios from 'axios';

export async function uploadFile(file: File): Promise<string> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await axios.post('/api/storage/upload', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });

  return response.data.fileName; // 返回文件标识
}

// 获取文件URL
export async function getFileUrl(fileName: string): Promise<string> {
  const response = await axios.get(`/api/storage/${fileName}/url`);
  return response.data.url;
}
```

### Vue 3 示例

```vue
<template>
  <div>
    <input type="file" @change="handleFileChange" accept="image/*,video/*" />
    <img v-if="previewUrl" :src="previewUrl" alt="Preview" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { uploadFile, getFileUrl } from './api/storage';

const previewUrl = ref<string>('');

async function handleFileChange(event: Event) {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  
  if (!file) return;
  
  try {
    // 上传文件
    const fileName = await uploadFile(file);
    
    // 获取访问URL
    const url = await getFileUrl(fileName);
    previewUrl.value = url;
  } catch (error) {
    console.error('Upload failed:', error);
  }
}
</script>
```

---

## 🛡️ 安全特性

### 1. 文件验证

```csharp
// 文件大小限制
if (file.Length > 50 * 1024 * 1024)
    return BadRequest("File size exceeds 50MB limit");

// 文件类型验证
var allowedTypes = new[] 
{ 
    "image/jpeg", "image/png", "image/gif", "image/webp",
    "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo"
};

if (!allowedTypes.Contains(file.ContentType?.ToLower()))
    return BadRequest("Invalid file type");
```

### 2. 预签名 URL

MinIO 生成的预签名 URL 包含临时访问令牌，无需暴露永久密钥：

```csharp
var url = await _minioClient.PresignedGetObjectAsync(args
    .WithBucket(_bucketName)
    .WithObject(fileName)
    .WithExpiry(60 * 60 * 24 * 7)); // 7天有效期
```

### 3. 认证保护

所有存储 API 端点都需要 JWT 认证：

```csharp
var group = app.MapGroup("/api/storage")
    .RequireAuthorization()  // 需要登录
    .WithTags("Storage");
```

---

## ⚡ AOT 兼容性

### MinIO SDK AOT 支持

CatCat 使用 **Minio 6.0.3** SDK，该版本对 AOT 编译支持良好。

### JSON 序列化

所有 API 响应类型都已在 `AppJsonContext` 中注册：

```csharp
[JsonSerializable(typeof(FileUploadResponse))]
[JsonSerializable(typeof(FileUrlResponse))]
public partial class AppJsonContext : JsonSerializerContext { }
```

### 注意事项

MinIO SDK 内部使用了一些反射，但主要用于 HTTP 客户端配置，不影响核心存储功能的 AOT 编译。

**建议**:
- ✅ 优先使用标准 API 方法（PutObject, GetObject, RemoveObject）
- ✅ 避免使用高级特性（Policy, Notification）
- ✅ 测试 AOT 编译后的运行时行为

---

## 🚀 性能优化

### 1. 文件命名策略

使用 GUID 生成唯一文件名，避免数据库查询：

```csharp
var extension = Path.GetExtension(fileName);
var uniqueFileName = $"{Guid.NewGuid()}{extension}";
```

### 2. 流式上传

直接使用文件流上传，无需加载到内存：

```csharp
using var stream = file.OpenReadStream();
await _minioClient.PutObjectAsync(putArgs
    .WithStreamData(stream)
    .WithObjectSize(stream.Length));
```

### 3. 并发控制

MinIO 默认支持高并发，但建议在应用层限制：

```csharp
// 在 Rate Limiting 中配置
builder.Services.AddRateLimiting(options =>
{
    options.AddPolicy("storage", new SlidingWindowRateLimiter(
        permitLimit: 10,
        window: TimeSpan.FromMinutes(1)));
});
```

---

## 📊 监控与日志

### 日志记录

```csharp
logger.LogInformation("User {UserId} uploaded file: {FileName}", userId, fileName);
logger.LogError(ex, "Failed to upload file for user {UserId}", userId);
```

### MinIO 指标

MinIO 提供 Prometheus 指标端点：

```
http://localhost:9000/minio/v2/metrics/cluster
```

---

## 🔍 故障排查

### 1. 连接失败

**症状**: `MinioClient` 无法连接到 MinIO

**解决方案**:
```bash
# 检查 MinIO 服务状态
docker ps | grep minio

# 检查端口
nc -zv localhost 9000

# 查看日志
docker logs catcat-minio
```

### 2. Bucket 不存在

**症状**: `The specified bucket does not exist`

**解决方案**:
- MinIO 服务会在启动时自动创建 Bucket
- 手动创建: 访问 Web 控制台 → Buckets → Create Bucket

### 3. 预签名 URL 过期

**症状**: 访问文件时返回 403

**解决方案**:
- 预签名 URL 有效期为 7 天
- 调用 `/api/storage/{fileName}/url` 重新获取 URL

---

## 📈 生产环境建议

### 1. 持久化存储

使用外部存储卷挂载：

```yaml
volumes:
  minio_data:
    driver: local
    driver_opts:
      type: none
      o: bind
      device: /path/to/minio/data
```

### 2. HTTPS 配置

生产环境建议启用 SSL：

```json
{
  "MinIO": {
    "Endpoint": "minio.example.com",
    "UseSSL": true
  }
}
```

### 3. 集群部署

MinIO 支持分布式部署（至少 4 节点）：

```bash
docker run -p 9000:9000 minio/minio server \
  http://minio{1...4}/data{1...4}
```

### 4. 备份策略

使用 MinIO Client (`mc`) 定期备份：

```bash
# 镜像同步
mc mirror catcat-minio/catcat-media backup-storage/catcat-media

# 增量备份
mc mirror --watch catcat-minio/catcat-media backup-storage/catcat-media
```

---

## 📚 相关文档

- [MinIO 官方文档](https://min.io/docs/minio/linux/index.html)
- [MinIO .NET SDK](https://github.com/minio/minio-dotnet)
- [S3 API 兼容性](https://docs.min.io/docs/minio-client-complete-guide.html)

---

**最后更新**: 2025-10-03  
**维护者**: CatCat Team

