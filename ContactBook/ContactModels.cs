public class ContactValidationException(string message) : Exception(message);

public record ContactRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? JobTitle,
    string? Category,
    bool IsFavorite,
    string? Notes
);

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNext,
    bool HasPrevious
);

public record ContactStats(
    int Total,
    int Favorites,
    int WorkCount,
    int PersonalCount,
    int FamilyCount,
    int OtherCount
);
