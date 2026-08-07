using System.Collections.Generic;

namespace Mauren.Discord.Application.Abstractions.Validation
{
    /// <summary>
    /// Defines a contract for validating requests (commands or queries) before they 
    /// are processed by a handler.
    /// </summary>
    /// 
    /// <typeparam name="TRequest">
    /// The type of the request being validated. This type parameter is contravariant.
    /// </typeparam>
    internal interface IValidator<in TRequest>
    {
        /// <summary>
        /// Validates the specified <paramref name="request"/> and returns a collection of error messages.
        /// </summary>
        /// 
        /// <param name="request">
        /// The request instance to evaluate against validation rules.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> containing <see cref="ValidationError"/>s.
        /// An empty collection indicates that validation passed successfully.
        /// </returns>
        IEnumerable<ValidationError> Validate(TRequest request);
    }
}
