namespace YaungMel_POS.shared.Responses;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public bool IsError { get { return !IsSuccess; } }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }

    public bool IsValidationError() => Type == EnumRespType.ValidationError;
    public bool IsSystemError() => Type == EnumRespType.SystemError;
    public bool IsNotFound() => Type == EnumRespType.NotFound;
    private EnumRespType Type { get; set; }

    public EnumRespType GetEnumRespType() => Type;




    public static Result<T> Success(T data, string message = "Success")
    {
        return new Result<T>()
        {
            IsSuccess = true,
            Type = EnumRespType.Success,
            Data = data,
            Message = message
        };
    }

    public static Result<T> DeleteSuccess(string message = "Deleting Successful.")
    {
        return new Result<T>()
        {
            IsSuccess = true,
            Type = EnumRespType.Success,
            Message = message
        };
    }

    public static Result<T> ValidationError(string message, T? data = default)
    {
        return new Result<T>()
        {
            IsSuccess = false,
            Type = EnumRespType.ValidationError,
            Data = data,
            Message = message
        };
    }


    public static Result<T> SystemError(string message, T? data = default)
    {
        return new Result<T>()
        {
            IsSuccess = false,
            Type = EnumRespType.SystemError,
            Data = data,
            Message = message
        };
    }


    public static Result<T> NotFound(string message, T? data = default)
    {
        return new Result<T>()
        {
            IsSuccess = false,
            Type = EnumRespType.NotFound,
            Data = data,
            Message = message
        };
    }


    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }

    public enum EnumRespType
    {
        None,
        Success,
        ValidationError,
        SystemError,
        NotFound
    }

}

