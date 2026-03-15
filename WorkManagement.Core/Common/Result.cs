namespace AuthSystem.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public List<string> Errors { get; private set; } = new();
        protected Result(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
        public static Result Success() => new(true, string.Empty);
        public static Result Fail(string error) => new(false, error);
        public static Result Fail(List<string> errors)
        {
            var result = new Result(false, string.Join("; ", errors));
            result.Errors = errors;
            return result;
        }
    }
    public class Result<T> : Result
    {
        public T? Data { get; private set; }
        private Result(bool isSuccess, T? data, string errorMessage)
            : base(isSuccess, errorMessage)
        {
            Data = data;
        }
        public static Result<T> Success(T data) => new(true, data, string.Empty);
        public new static Result<T> Fail(string error) => new(false, default, error);
    }
}
