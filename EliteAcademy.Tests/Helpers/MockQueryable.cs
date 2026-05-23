namespace EliteAcademy.Tests.Helpers;

public static class MockQueryable
{
    public static IQueryable<T> Of<T>(params T[] items) =>
        items.AsQueryable();
}
