# Aspire Kubernetes Deployment Guide

This guide explains how to deploy the CatCat application to Kubernetes using .NET Aspire.

## 📋 Prerequisites

1. **.NET 9.0 SDK** with Aspire workload
   ```bash
   dotnet workload install aspire
   ```

2. **Docker** - For building container images
   ```bash
   docker --version
   ```

3. **kubectl** - Kubernetes CLI
   ```bash
   kubectl version --client
   ```

4. **Kubernetes Cluster** - One of:
   - Azure Kubernetes Service (AKS)
   - Amazon Elastic Kubernetes Service (EKS)
   - Google Kubernetes Engine (GKE)
   - Minikube (for local testing)
   - Kind (Kubernetes in Docker)

5. **Container Registry** - One of:
   - Azure Container Registry (ACR)
   - Docker Hub
   - Amazon ECR
   - Google Container Registry

## 🚀 Quick Deployment

### Windows (PowerShell)

```powershell
# Deploy with default settings
.\scripts\deploy-to-k8s.ps1 -Registry "your-registry.azurecr.io"

# Custom namespace
.\scripts\deploy-to-k8s.ps1 -Registry "myregistry.azurecr.io" -Namespace "production"

# Skip build (images already built)
.\scripts\deploy-to-k8s.ps1 -Registry "myregistry.azurecr.io" -Build:$false

# Only build, don't push or deploy
.\scripts\deploy-to-k8s.ps1 -Push:$false -Deploy:$false
```

### Linux/macOS (Bash)

```bash
# Make script executable
chmod +x scripts/deploy-to-k8s.sh

# Deploy with default settings
./scripts/deploy-to-k8s.sh your-registry.azurecr.io catcat

# With environment variables
BUILD=true PUSH=true DEPLOY=true ./scripts/deploy-to-k8s.sh
```

## 📦 Manual Deployment Steps

### 1. Login to Container Registry

**Azure Container Registry:**
```bash
az acr login --name yourregistry
```

**Docker Hub:**
```bash
docker login
```

### 2. Build and Push Images

```bash
# Build API image
docker build -t your-registry/catcat-api:latest -f src/CatCat.API/Dockerfile .

# Build Web image
docker build -t your-registry/catcat-web:latest -f src/CatCat.Web/Dockerfile .

# Push images
docker push your-registry/catcat-api:latest
docker push your-registry/catcat-web:latest
```

### 3. Generate Kubernetes Manifests with Aspire

```bash
cd src/CatCat.AppHost

# Generate K8s manifests
dotnet publish /t:GenerateKubernetesManifests \
    /p:Configuration=Release \
    /p:ContainerRegistry=your-registry.azurecr.io \
    /p:KubernetesNamespace=catcat
```

### 4. Deploy to Kubernetes

```bash
# Create namespace
kubectl create namespace catcat

# Apply manifests
kubectl apply -f src/CatCat.AppHost/bin/Release/net9.0/manifest.yaml -n catcat
```

## 🔧 Aspire Configuration

### AppHost Configuration

The `CatCat.AppHost/Program.cs` is configured for Kubernetes deployment:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("catcatdb");

var redis = builder.AddRedis("redis")
    .WithRedisCommander();

var nats = builder.AddNats("nats")
    .WithJetStream();

// Application
var api = builder.AddProject<Projects.CatCat_API>("api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(nats);

var web = builder.AddNpmApp("web", "../CatCat.Web")
    .WithReference(api)
    .WithHttpEndpoint(env: "API_URL");

builder.Build().Run();
```

### Kubernetes-Specific Settings

Add to `CatCat.AppHost/appsettings.json`:

```json
{
  "Aspire": {
    "Deployment": {
      "Publisher": "kubernetes",
      "Kubernetes": {
        "Namespace": "catcat",
        "ImagePullPolicy": "Always",
        "Resources": {
          "API": {
            "Limits": {
              "Cpu": "1000m",
              "Memory": "1Gi"
            },
            "Requests": {
              "Cpu": "250m",
              "Memory": "256Mi"
            }
          },
          "Web": {
            "Limits": {
              "Cpu": "500m",
              "Memory": "512Mi"
            },
            "Requests": {
              "Cpu": "100m",
              "Memory": "128Mi"
            }
          }
        },
        "Replicas": {
          "API": 3,
          "Web": 2
        },
        "AutoScaling": {
          "Enabled": true,
          "MinReplicas": 2,
          "MaxReplicas": 10,
          "TargetCPUUtilization": 70
        }
      }
    }
  }
}
```

## 📊 Monitoring Deployment

### Check Pod Status

```bash
kubectl get pods -n catcat
```

### View Logs

```bash
# API logs
kubectl logs -f deployment/catcat-api -n catcat

# Web logs
kubectl logs -f deployment/catcat-web -n catcat

# All logs
kubectl logs -f -l app.kubernetes.io/part-of=catcat -n catcat
```

### Check Services

```bash
kubectl get svc -n catcat
```

### Port Forwarding (for testing)

```bash
# Forward API
kubectl port-forward svc/catcat-api 8080:80 -n catcat

# Forward Web
kubectl port-forward svc/catcat-web 3000:80 -n catcat
```

## 🔐 Secrets Management

### Create Kubernetes Secrets

```bash
# Database credentials
kubectl create secret generic catcat-db-secret \
  --from-literal=username=postgres \
  --from-literal=password=your-password \
  -n catcat

# Stripe API key
kubectl create secret generic catcat-stripe-secret \
  --from-literal=api-key=your-stripe-key \
  -n catcat

# JWT secret
kubectl create secret generic catcat-jwt-secret \
  --from-literal=secret-key=your-jwt-secret \
  -n catcat
```

### Reference in Aspire

```csharp
var api = builder.AddProject<Projects.CatCat_API>("api")
    .WithEnvironment("Stripe__SecretKey", builder.AddSecret("stripe-secret"))
    .WithEnvironment("Jwt__SecretKey", builder.AddSecret("jwt-secret"));
```

## 🌐 Ingress Configuration

### Nginx Ingress

```bash
# Install Nginx Ingress Controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml
```

Aspire will automatically generate Ingress resources based on your configuration.

### Custom Domain

Update `appsettings.json`:

```json
{
  "Aspire": {
    "Deployment": {
      "Kubernetes": {
        "Ingress": {
          "Enabled": true,
          "Host": "catcat.yourdomain.com",
          "TLS": {
            "Enabled": true,
            "SecretName": "catcat-tls"
          }
        }
      }
    }
  }
}
```

## 🔄 Update Deployment

### Rolling Update

```bash
# Update API image
docker build -t your-registry/catcat-api:v2 -f src/CatCat.API/Dockerfile .
docker push your-registry/catcat-api:v2

# Update deployment
kubectl set image deployment/catcat-api api=your-registry/catcat-api:v2 -n catcat

# Check rollout status
kubectl rollout status deployment/catcat-api -n catcat
```

### Rollback

```bash
# Rollback to previous version
kubectl rollout undo deployment/catcat-api -n catcat

# Rollback to specific revision
kubectl rollout undo deployment/catcat-api --to-revision=2 -n catcat
```

## 🧹 Cleanup

### Remove Deployment

```bash
# Delete namespace (removes everything)
kubectl delete namespace catcat

# Or delete specific resources
kubectl delete all -l app.kubernetes.io/part-of=catcat -n catcat
```

## 📈 Scaling

### Manual Scaling

```bash
# Scale API
kubectl scale deployment catcat-api --replicas=5 -n catcat

# Scale Web
kubectl scale deployment catcat-web --replicas=3 -n catcat
```

### Auto-scaling (HPA)

Aspire automatically configures HorizontalPodAutoscaler based on `appsettings.json`.

View HPA status:
```bash
kubectl get hpa -n catcat
```

## 🔍 Troubleshooting

### Pod Not Starting

```bash
# Describe pod
kubectl describe pod <pod-name> -n catcat

# Check events
kubectl get events -n catcat --sort-by='.lastTimestamp'
```

### Image Pull Errors

```bash
# Create image pull secret for private registry
kubectl create secret docker-registry regcred \
  --docker-server=your-registry.azurecr.io \
  --docker-username=<username> \
  --docker-password=<password> \
  -n catcat
```

### Database Connection Issues

```bash
# Test PostgreSQL connectivity
kubectl run -it --rm debug --image=postgres:17 --restart=Never -n catcat -- \
  psql -h postgres-service -U postgres -d catcat
```

## 🌟 Production Best Practices

1. **Use Specific Image Tags** - Avoid `:latest` in production
   ```bash
   docker build -t your-registry/catcat-api:1.0.0 .
   ```

2. **Enable Resource Limits** - Configure in `appsettings.json`

3. **Configure Health Checks** - Aspire automatically adds liveness/readiness probes

4. **Use Secrets** - Never hardcode credentials

5. **Enable Monitoring** - Use OpenTelemetry with Aspire Dashboard

6. **Backup Database** - Regular PostgreSQL backups

7. **Use Ingress with TLS** - Enable HTTPS

8. **Configure Pod Disruption Budgets** - For high availability

## 📚 Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire Kubernetes Deployment](https://learn.microsoft.com/dotnet/aspire/deployment/kubernetes)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [kubectl Cheat Sheet](https://kubernetes.io/docs/reference/kubectl/cheatsheet/)

