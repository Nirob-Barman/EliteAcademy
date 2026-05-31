namespace EliteAcademy.Domain.Common
{
    public class DomainResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Value { get; private set; }
        public string Error { get; private set; } = string.Empty;

        public static DomainResult<T> Ok(T value) => new() { IsSuccess = true, Value = value };
        public static DomainResult<T> Fail(string error) => new() { IsSuccess = false, Error = error };
    }
}
