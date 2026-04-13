namespace ArtAuction.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; init; }
    public string[] Errors { get; init; } = [];

    public static Result Success()
    {
        return new Result { Succeeded = true };
    }

    public static Result Failure(params string[] errors)
    {
        return new Result
        {
            Succeeded = false,
            Errors = errors
        };
    }
}

public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            Succeeded = true,
            Data = data
        };
    }

    public new static Result<T> Failure(params string[] errors)
    {
        return new Result<T>
        {
            Succeeded = false,
            Errors = errors
        };
    }
}
