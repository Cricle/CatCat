using CatCat.Transit.Configuration;

namespace CatCat.Transit.Tests.Configuration;

public class TransitOptionsTests
{
    [Fact]
    public void DefaultOptions_HasReasonableDefaults()
    {
        // Arrange & Act
        var options = new TransitOptions();

        // Assert
        options.EnableLogging.Should().BeTrue();
        options.EnableIdempotency.Should().BeTrue();
        options.EnableRetry.Should().BeTrue();
        options.MaxRetryAttempts.Should().BeGreaterThan(0);
        options.IdempotencyShardCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void WithHighPerformance_ConfiguresCorrectly()
    {
        // Arrange
        var options = new TransitOptions();

        // Act
        options.WithHighPerformance();

        // Assert
        options.MaxConcurrentRequests.Should().Be(5000);
        options.EnableCircuitBreaker.Should().BeFalse();
        options.EnableRateLimiting.Should().BeFalse();
        options.EnableRetry.Should().BeFalse();
        options.EnableIdempotency.Should().BeTrue(); // Still enabled for safety
    }

    [Fact]
    public void WithResilience_EnablesAllFeatures()
    {
        // Arrange
        var options = new TransitOptions();

        // Act
        options.WithResilience();

        // Assert
        options.EnableCircuitBreaker.Should().BeTrue();
        options.EnableRateLimiting.Should().BeTrue();
        options.EnableRetry.Should().BeTrue();
        options.EnableIdempotency.Should().BeTrue();
        options.EnableDeadLetterQueue.Should().BeTrue();
    }

    [Fact]
    public void ForDevelopment_DisablesProductionFeatures()
    {
        // Arrange
        var options = new TransitOptions();

        // Act
        options.ForDevelopment();

        // Assert
        options.EnableCircuitBreaker.Should().BeFalse();
        options.EnableRateLimiting.Should().BeFalse();
        options.EnableIdempotency.Should().BeFalse();
        options.EnableTracing.Should().BeTrue();
        options.EnableLogging.Should().BeTrue();
    }

    [Fact]
    public void Minimal_DisablesMostFeatures()
    {
        // Arrange
        var options = new TransitOptions();

        // Act
        options.Minimal();

        // Assert
        options.EnableLogging.Should().BeFalse();
        options.EnableTracing.Should().BeFalse();
        options.EnableIdempotency.Should().BeFalse();
        options.EnableRetry.Should().BeFalse();
        options.EnableCircuitBreaker.Should().BeFalse();
    }

    [Fact]
    public void ChainedConfiguration_Works()
    {
        // Arrange & Act
        var options = new TransitOptions()
            .WithResilience();
        
        options.MaxConcurrentRequests = 100;
        options.TimeoutSeconds = 60;

        // Assert
        options.EnableCircuitBreaker.Should().BeTrue();
        options.EnableRateLimiting.Should().BeTrue();
        options.MaxConcurrentRequests.Should().Be(100);
        options.TimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public void CustomConfiguration_OverridesDefaults()
    {
        // Arrange
        var options = new TransitOptions();

        // Act
        options.MaxRetryAttempts = 10;
        options.RetryDelayMs = 500;
        options.IdempotencyRetentionHours = 48;

        // Assert
        options.MaxRetryAttempts.Should().Be(10);
        options.RetryDelayMs.Should().Be(500);
        options.IdempotencyRetentionHours.Should().Be(48);
    }
}

