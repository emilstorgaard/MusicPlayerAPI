namespace MusicPlayerAPI.Models
{
    public class StatusResult<T>
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public static StatusResult<T> Success(T data, int status, string? message = null)
        {
            return new StatusResult<T>
            {
                Status = status,
                Data = data,
                Message = message ?? "Operation successful"
            };
        }

        public static StatusResult<T> Failure(int status, string message)
        {
            return new StatusResult<T>
            {
                Status = status,
                Message = message
            };
        }
    }
}
