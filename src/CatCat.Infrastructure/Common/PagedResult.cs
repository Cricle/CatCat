namespace CatCat.Infrastructure.Common;

public record PagedResult<T>(IEnumerable<T> Items, int Total)
{
    public int Count => Items.Count();
}

public record ReviewPagedResult(
    IEnumerable<Entities.Review> Items,
    int Total,
    decimal AverageRating)
{
    public int Count => Items.Count();
}

