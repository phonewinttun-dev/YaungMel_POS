using System.Text.Json.Serialization;

namespace YaungMel_POS.Shared;

public enum EnumRespType
{
    None,
    Success,
    Failure,
    ValidationError,
    SystemError,
    NotFound
}

public class Result
{
    public bool IsSuccess { get; set; }
    public bool IsError { get { return !IsSuccess; } }
    public string Message { get; set; } = null!;
    public EnumRespType Type { get; set; }

    [JsonConstructor]
    public Result(bool isSuccess, string message, EnumRespType type = EnumRespType.None)
    {
        IsSuccess = isSuccess;
        Message = message;
        Type = type;
    }
    public static Result Success(string message = "Success")
         => new(true, message, EnumRespType.Success);
    public static Result Failure(string message, EnumRespType type = EnumRespType.SystemError)
        => new(false, message, type);
    public bool IsValidationError() => Type == EnumRespType.ValidationError;
    public bool IsSystemError() => Type == EnumRespType.SystemError;
    public bool IsNotFound() => Type == EnumRespType.NotFound;
    public EnumRespType GetEnumRespType() => Type;
}

public class Result<T> : Result
{
    public T? Data { get; private set; }

    [JsonConstructor]
    public Result(bool isSuccess, string message, EnumRespType type, T? data)
    : base(isSuccess, message, type)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string message = "Success")
        => new(true, message, EnumRespType.Success, data);

    public static Result<T> DeleteSuccess(string message = "Deleted successfully!")
       => new(true, message, EnumRespType.Success, default);
    public static Result<T> Failure(string message, EnumRespType type = EnumRespType.Failure)
        => new(false, message, type, default);

    public static Result<T> ValidationError(string message, T? data = default)
        => new(false, message, EnumRespType.ValidationError, data);

    public static Result<T> SystemError(string message, T? data = default)
        => new(false, message, EnumRespType.SystemError, data);

    public static Result<T> NotFound(string message, T? data = default)
        => new(false, message, EnumRespType.NotFound, data);
}
