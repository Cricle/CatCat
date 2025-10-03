#!/bin/bash
# Aspire Deploy to Kubernetes Script (Linux/macOS)

set -e

# Default values
REGISTRY="${1:-your-registry.azurecr.io}"
NAMESPACE="${2:-catcat}"
BUILD="${BUILD:-true}"
PUSH="${PUSH:-true}"
DEPLOY="${DEPLOY:-true}"

echo ""
echo "╔═══════════════════════════════════════════════════════════════╗"
echo "║   🚀 Aspire Deploy to Kubernetes              ║"
echo "╚═══════════════════════════════════════════════════════════════╝"
echo ""

# Check prerequisites
echo "📋 Checking prerequisites..."

for cmd in dotnet docker kubectl; do
    if ! command -v $cmd &> /dev/null; then
        echo "❌ Error: $cmd is not installed"
        exit 1
    fi
done
echo "✅ All prerequisites installed"
echo ""

# Check .NET Aspire workload
echo "🔍 Checking .NET Aspire workload..."
if ! dotnet workload list | grep -q "aspire"; then
    echo "⚠️ .NET Aspire workload not installed. Installing..."
    dotnet workload install aspire
    echo "✅ Aspire workload installed"
else
    echo "✅ Aspire workload already installed"
fi
echo ""

# Build container images
if [ "$BUILD" = "true" ]; then
    echo "🔨 Building container images..."
    
    # Build API
    echo "  📦 Building CatCat.API..."
    docker build -t "$REGISTRY/catcat-api:latest" -f src/CatCat.API/Dockerfile .
    
    # Build Web
    echo "  📦 Building CatCat.Web..."
    docker build -t "$REGISTRY/catcat-web:latest" -f src/CatCat.Web/Dockerfile .
    
    echo "✅ Images built successfully"
    echo ""
fi

# Push images to registry
if [ "$PUSH" = "true" ]; then
    echo "📤 Pushing images to registry..."
    
    docker push "$REGISTRY/catcat-api:latest"
    docker push "$REGISTRY/catcat-web:latest"
    
    echo "✅ Images pushed successfully"
    echo ""
fi

# Deploy to Kubernetes using Aspire
if [ "$DEPLOY" = "true" ]; then
    echo "🚀 Deploying to Kubernetes with Aspire..."
    
    # Create namespace if not exists
    kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -
    
    # Generate Kubernetes manifests using Aspire
    echo "  📝 Generating Kubernetes manifests..."
    cd src/CatCat.AppHost
    
    # Publish Aspire project to generate K8s manifests
    dotnet publish /t:GenerateKubernetesManifests \
        /p:Configuration=Release \
        /p:ContainerRegistry=$REGISTRY \
        /p:KubernetesNamespace=$NAMESPACE
    
    cd ../..
    
    # Apply manifests
    echo "  🔧 Applying Kubernetes manifests..."
    MANIFEST_PATH="src/CatCat.AppHost/bin/Release/net9.0/manifest.yaml"
    
    if [ -f "$MANIFEST_PATH" ]; then
        kubectl apply -f $MANIFEST_PATH -n $NAMESPACE
        echo "✅ Manifests applied successfully"
    else
        echo "⚠️ Manifest file not found. Using dotnet run to deploy..."
        
        # Alternative: Use Aspire CLI to deploy
        cd src/CatCat.AppHost
        dotnet run --publisher kubernetes --output-path ./k8s-manifests
        cd ../..
        
        if [ -d "src/CatCat.AppHost/k8s-manifests" ]; then
            kubectl apply -f src/CatCat.AppHost/k8s-manifests -n $NAMESPACE
            echo "✅ Deployed using Aspire CLI"
        fi
    fi
    echo ""
fi

# Show deployment status
echo "📊 Deployment Status:"
kubectl get all -n $NAMESPACE

echo ""
echo "╔═══════════════════════════════════════════════════════════════╗"
echo "║   ✅ Deployment Complete!                     ║"
echo "╚═══════════════════════════════════════════════════════════════╝"
echo ""

echo "🔗 Useful commands:"
echo "  • View pods:     kubectl get pods -n $NAMESPACE"
echo "  • View services: kubectl get svc -n $NAMESPACE"
echo "  • View logs:     kubectl logs -f deployment/catcat-api -n $NAMESPACE"
echo "  • Port forward:  kubectl port-forward svc/catcat-api 8080:80 -n $NAMESPACE"
echo ""

