using CatCat.Transit.DeadLetter;
using CatCat.Transit.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CatCat.Transit.Tests.DeadLetter;

public class DeadLetterQueueTests
{
    [Fact]
    public async Task SendAsync_StoresFailedMessage()
    {
        // Arrange
        var logger = NullLogger<InMemoryDeadLetterQueue>.Instance;
        var queue = new InMemoryDeadLetterQueue(logger, maxSize: 100);
        var message = new TestCommand("test");
        var exception = new InvalidOperationException("test error");

        // Act
        await queue.SendAsync(message, exception, retryCount: 3);

        // Assert
        var messages = await queue.GetFailedMessagesAsync(maxCount: 10);
        messages.Should().HaveCount(1);
        messages[0].MessageId.Should().Be(message.MessageId);
        messages[0].RetryCount.Should().Be(3);
        messages[0].ExceptionMessage.Should().Contain("test error");
    }

    [Fact]
    public async Task GetFailedMessagesAsync_ReturnsStoredMessages()
    {
        // Arrange
        var logger = NullLogger<InMemoryDeadLetterQueue>.Instance;
        var queue = new InMemoryDeadLetterQueue(logger, maxSize: 100);
        
        await queue.SendAsync(new TestCommand("msg1"), new Exception("err1"), 1);
        await queue.SendAsync(new TestCommand("msg2"), new Exception("err2"), 2);

        // Act
        var messages = await queue.GetFailedMessagesAsync(maxCount: 10);

        // Assert
        messages.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFailedMessagesAsync_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        var logger = NullLogger<InMemoryDeadLetterQueue>.Instance;
        var queue = new InMemoryDeadLetterQueue(logger, maxSize: 100);
        
        for (int i = 0; i < 10; i++)
        {
            await queue.SendAsync(new TestCommand($"msg{i}"), new Exception(), 1);
        }

        // Act
        var messages = await queue.GetFailedMessagesAsync(maxCount: 5);

        // Assert
        messages.Should().HaveCount(5);
    }

    [Fact]
    public async Task SendAsync_ExceedsMaxSize_DropsOldest()
    {
        // Arrange
        var logger = NullLogger<InMemoryDeadLetterQueue>.Instance;
        var queue = new InMemoryDeadLetterQueue(logger, maxSize: 3);
        
        // Act - add 5 messages to queue with max size 3
        for (int i = 0; i < 5; i++)
        {
            await queue.SendAsync(new TestCommand($"msg{i}"), new Exception(), 1);
        }

        // Assert
        var messages = await queue.GetFailedMessagesAsync(maxCount: 10);
        messages.Should().HaveCount(3); // Only keeps last 3
    }

    [Fact]
    public async Task GetFailedMessagesAsync_IncludesAllProperties()
    {
        // Arrange
        var logger = NullLogger<InMemoryDeadLetterQueue>.Instance;
        var queue = new InMemoryDeadLetterQueue(logger, maxSize: 100);
        var message = new TestCommand("test data");
        var exception = new InvalidOperationException("error message");
        
        // Act
        await queue.SendAsync(message, exception, retryCount: 5);
        var messages = await queue.GetFailedMessagesAsync(maxCount: 1);

        // Assert
        var dlqMessage = messages.First();
        dlqMessage.MessageId.Should().Be(message.MessageId);
        dlqMessage.MessageType.Should().Be("TestCommand");
        dlqMessage.RetryCount.Should().Be(5);
        dlqMessage.ExceptionType.Should().Be("InvalidOperationException");
        dlqMessage.ExceptionMessage.Should().Contain("error message");
        dlqMessage.FailedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        dlqMessage.MessageJson.Should().NotBeEmpty();
        dlqMessage.StackTrace.Should().NotBeNull();
    }

    [Fact]
    public async Task Parallel_SendAsync_ThreadSafe()
    {
        // Arrange
        var logger = NullLogger<InMemoryDeadLetterQueue>.Instance;
        var queue = new InMemoryDeadLetterQueue(logger, maxSize: 1000);

        // Act
        var tasks = Enumerable.Range(0, 100).Select(i =>
            queue.SendAsync(new TestCommand($"msg{i}"), new Exception(), 1));
        
        await Task.WhenAll(tasks);

        // Assert
        var messages = await queue.GetFailedMessagesAsync(maxCount: 1000);
        messages.Should().HaveCount(100);
    }

    [Fact]
    public async Task SendAsync_PreservesExceptionDetails()
    {
        // Arrange
        var logger = NullLogger<InMemoryDeadLetterQueue>.Instance;
        var queue = new InMemoryDeadLetterQueue(logger, maxSize: 100);
        var message = new TestCommand("test");
        
        Exception exception;
        try
        {
            throw new InvalidOperationException("Specific error with stack trace");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        await queue.SendAsync(message, exception, retryCount: 1);

        // Assert
        var messages = await queue.GetFailedMessagesAsync(maxCount: 1);
        var dlqMessage = messages.First();
        dlqMessage.ExceptionMessage.Should().Be("Specific error with stack trace");
        dlqMessage.ExceptionType.Should().Be("InvalidOperationException");
        dlqMessage.StackTrace.Should().Contain("DeadLetterQueueTests");
    }

    [Fact]
    public void Constructor_DefaultMaxSize_Works()
    {
        // Arrange & Act
        var logger = NullLogger<InMemoryDeadLetterQueue>.Instance;
        var queue = new InMemoryDeadLetterQueue(logger);

        // Assert
        queue.Should().NotBeNull();
    }

    [Fact]
    public async Task EmptyQueue_GetFailedMessagesAsync_ReturnsEmpty()
    {
        // Arrange
        var logger = NullLogger<InMemoryDeadLetterQueue>.Instance;
        var queue = new InMemoryDeadLetterQueue(logger, maxSize: 100);

        // Act
        var messages = await queue.GetFailedMessagesAsync(maxCount: 10);

        // Assert
        messages.Should().BeEmpty();
    }
}

