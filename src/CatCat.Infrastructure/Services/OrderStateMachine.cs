using CatCat.Infrastructure.Entities;

namespace CatCat.Infrastructure.Services;

/// <summary>
/// Order state machine for validating status transitions
/// </summary>
public static class OrderStateMachine
{
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Queued] = new() { OrderStatus.Pending, OrderStatus.Cancelled },
        [OrderStatus.Pending] = new() { OrderStatus.Accepted, OrderStatus.Cancelled },
        [OrderStatus.Accepted] = new() { OrderStatus.InProgress, OrderStatus.Cancelled },
        [OrderStatus.InProgress] = new() { OrderStatus.Completed, OrderStatus.Cancelled },
        [OrderStatus.Completed] = new(), // Terminal state
        [OrderStatus.Cancelled] = new()  // Terminal state
    };

    public static bool IsValidTransition(OrderStatus from, OrderStatus to)
    {
        return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static List<OrderStatus> GetAllowedNextStates(OrderStatus current)
    {
        return AllowedTransitions.GetValueOrDefault(current, new List<OrderStatus>());
    }

    public static bool IsTerminalState(OrderStatus status)
    {
        return status == OrderStatus.Completed || status == OrderStatus.Cancelled;
    }

    public static bool CanBeCancelled(OrderStatus status)
    {
        return status != OrderStatus.Completed && status != OrderStatus.Cancelled;
    }
}

