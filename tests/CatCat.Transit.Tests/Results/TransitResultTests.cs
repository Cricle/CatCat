using CatCat.Transit.Results;

namespace CatCat.Transit.Tests.Results;

public class TransitResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Act
        var result = TransitResult<string>.Success("test value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
        result.Error.Should().BeNull();
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void Failure_CreatesFailureResult()
    {
        // Act
        var result = TransitResult<string>.Failure("error message");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be("error message");
    }

    [Fact]
    public void Failure_WithException_StoresException()
    {
        // Arrange
        var innerException = new InvalidOperationException("test error");
        var transitException = new TransitException("error", innerException);

        // Act
        var result = TransitResult<string>.Failure("error", transitException);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("error");
        result.Exception.Should().BeSameAs(transitException);
    }

    [Fact]
    public void Success_WithMetadata_StoresMetadata()
    {
        // Arrange
        var metadata = new ResultMetadata();
        metadata.Add("key1", "value1");
        metadata.Add("key2", "value2");

        // Act
        var result = TransitResult<string>.Success("value", metadata);

        // Assert
        result.Metadata.Should().NotBeNull();
        result.Metadata!.ContainsKey("key1").Should().BeTrue();
        result.Metadata.TryGetValue("key1", out var value1).Should().BeTrue();
        value1.Should().Be("value1");
    }

    [Fact]
    public void NonGeneric_Success_CreatesSuccessResult()
    {
        // Act
        var result = TransitResult.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void NonGeneric_Failure_CreatesFailureResult()
    {
        // Act
        var result = TransitResult.Failure("error");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("error");
    }

    [Fact]
    public void Metadata_IsReadOnly()
    {
        // Arrange
        var metadata = new ResultMetadata();
        metadata.Add("key", "value");
        var result = TransitResult<string>.Success("value", metadata);

        // Assert
        result.Metadata.Should().NotBeNull();
        result.Metadata.Should().BeSameAs(metadata);
    }
}

