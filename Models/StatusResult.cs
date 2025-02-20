public class StatusResult
{
    public int Status { get; set; }
    public string? Message { get; set; }

    public static StatusResult Success(int status, string? message = null)
    {
        return new StatusResult
        {
            Status = status,
            Message = message ?? "Successful"
        };
    }

    public static StatusResult Failure(int status, string message)
    {
        return new StatusResult
        {
            Status = status,
            Message = message ?? "Failed"
        };
    }
}

public class StatusResult<T> : StatusResult
{
    public T? Data { get; set; }

    public static StatusResult<T> Success(T data, int status, string? message = null)
    {
        return new StatusResult<T>
        {
            Status = status,
            Data = data,
            Message = message ?? "Successful"
        };
    }

    public static new StatusResult<T> Failure(int status, string message)
    {
        return new StatusResult<T>
        {
            Status = status,
            Message = message ?? "Failed"
        };
    }
}
