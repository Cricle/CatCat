namespace CatCat.Infrastructure.Entities;

public class ServiceProgress
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ServiceProviderId { get; set; }
    public ServiceProgressStatus Status { get; set; }
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? ImageUrls { get; set; } // JSON array of image URLs
    public DateTime CreatedAt { get; set; }
}

public enum ServiceProgressStatus
{
    OnTheWay = 1,        // On the way
    Arrived = 2,         // Arrived at location
    StartService = 3,    // Start service
    Feeding = 4,         // Feeding
    CleaningLitter = 5,  // Cleaning litter box
    Playing = 6,         // Playing with cat
    Grooming = 7,        // Grooming
    TakingPhotos = 8,    // Taking photos
    Completed = 9        // Service completed
}

