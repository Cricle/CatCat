using CatCat.Infrastructure.Entities;

namespace CatCat.Infrastructure.Services;

/// <summary>
/// Service progress state machine for validating progress transitions
/// </summary>
public static class ProgressStateMachine
{
    private static readonly Dictionary<ServiceProgressStatus, List<ServiceProgressStatus>> AllowedTransitions = new()
    {
        [ServiceProgressStatus.OnTheWay] = new() { ServiceProgressStatus.Arrived },
        [ServiceProgressStatus.Arrived] = new() { ServiceProgressStatus.StartService },
        [ServiceProgressStatus.StartService] = new()
        {
            ServiceProgressStatus.Feeding,
            ServiceProgressStatus.CleaningLitter,
            ServiceProgressStatus.Playing,
            ServiceProgressStatus.Grooming
        },
        [ServiceProgressStatus.Feeding] = new()
        {
            ServiceProgressStatus.CleaningLitter,
            ServiceProgressStatus.Playing,
            ServiceProgressStatus.Grooming,
            ServiceProgressStatus.TakingPhotos,
            ServiceProgressStatus.Completed
        },
        [ServiceProgressStatus.CleaningLitter] = new()
        {
            ServiceProgressStatus.Feeding,
            ServiceProgressStatus.Playing,
            ServiceProgressStatus.Grooming,
            ServiceProgressStatus.TakingPhotos,
            ServiceProgressStatus.Completed
        },
        [ServiceProgressStatus.Playing] = new()
        {
            ServiceProgressStatus.Feeding,
            ServiceProgressStatus.CleaningLitter,
            ServiceProgressStatus.Grooming,
            ServiceProgressStatus.TakingPhotos,
            ServiceProgressStatus.Completed
        },
        [ServiceProgressStatus.Grooming] = new()
        {
            ServiceProgressStatus.Feeding,
            ServiceProgressStatus.CleaningLitter,
            ServiceProgressStatus.Playing,
            ServiceProgressStatus.TakingPhotos,
            ServiceProgressStatus.Completed
        },
        [ServiceProgressStatus.TakingPhotos] = new()
        {
            ServiceProgressStatus.Feeding,
            ServiceProgressStatus.CleaningLitter,
            ServiceProgressStatus.Playing,
            ServiceProgressStatus.Grooming,
            ServiceProgressStatus.Completed
        },
        [ServiceProgressStatus.Completed] = new() // Terminal state
    };

    public static bool IsValidTransition(ServiceProgressStatus from, ServiceProgressStatus to)
    {
        // First progress can be any status
        // Subsequent progresses must follow transition rules
        return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static bool IsValidFirstStatus(ServiceProgressStatus status)
    {
        return status == ServiceProgressStatus.OnTheWay;
    }

    public static bool IsTerminalStatus(ServiceProgressStatus status)
    {
        return status == ServiceProgressStatus.Completed;
    }
}

