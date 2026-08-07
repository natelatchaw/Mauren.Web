using System;
using System.Diagnostics.CodeAnalysis;

namespace Mauren.Discord.Application.Abstractions.Messaging
{
    public class Result
    {
        public Boolean IsSuccess { get; set; }
        public String? Error { get; set; }

        protected Result(Boolean isSuccess, String? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true, null);
        public static Result Failure(String error) => new Result(false, error);
    }

    public class Result<T> : Result
    {
        public T? Value { get; set; }

        protected Result(T? value, Boolean isSuccess, String? error) : base(isSuccess, error)
        {
            Value = value;
        }

        public Boolean TryGetValue([NotNullWhen(true)] out T? value)
        {
            value = Value;
            return IsSuccess;
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, null);
        public static Result<T> Failure(String error) => new Result<T>(default, false, error);
    }
}
