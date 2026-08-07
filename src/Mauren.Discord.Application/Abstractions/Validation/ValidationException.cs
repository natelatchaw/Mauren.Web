using System;
using System.Collections.Generic;
using System.Linq;

namespace Mauren.Discord.Application.Abstractions.Validation
{
    /// <summary>
    /// An <see cref="Exception"/> representing one or more validation failures.
    /// </summary>
    public class ValidationException : AggregateException
    {
        /// <summary>
        /// The collection of errors represented by this exception.
        /// </summary>
        public IEnumerable<ValidationError> Errors { get; }

        /// <summary>
        /// Constructs a new <see cref="Exception"/> from the provided collection 
        /// of <see cref="ValidationError"/>s.
        /// </summary>
        /// 
        /// <param name="errors">
        /// A collection of <see cref="ValidationError"/>s.
        /// </param>
        public ValidationException(params IEnumerable<ValidationError> errors) : base(Convert(errors))
        {
            Errors = errors;
        }

        /// <summary>
        /// Converts a collection of <see cref="ValidationError"/>s to 
        /// a collection of <see cref="Exception"/>s.
        /// </summary>
        /// 
        /// <param name="errors">
        /// A collection of <see cref="ValidationError"/>s.
        /// </param>
        /// 
        /// <returns>
        /// A collection of <see cref="Exception"/>s.
        /// </returns>
        private static IEnumerable<Exception> Convert(IEnumerable<ValidationError> errors) => errors
            .Select((ValidationError error) => new Exception(error.ErrorMessage));
    }
}
