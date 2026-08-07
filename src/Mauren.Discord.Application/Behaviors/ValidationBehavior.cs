using Mauren.Discord.Application.Abstractions.Messaging;
using Mauren.Discord.Application.Abstractions.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mauren.Discord.Application.Behaviors
{
    /// <summary>
    /// A pipeline behavior that intercepts <typeparamref name="TRequest"/>s to perform validation 
    /// before they reach their designated handler.
    /// </summary>
    /// 
    /// <typeparam name="TRequest">
    /// The <see langword="type"/> of the request being processed.
    /// Must not be <see langword="null"/>.
    /// </typeparam>
    internal sealed class ValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
        where TRequest : notnull
    {
        /// <summary>
        /// A collection of <see cref="IValidator{TRequest}"/>s to utilize.
        /// </summary>
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResult}"/>
        /// <see langword="class"/>.
        /// </summary>
        /// 
        /// <param name="validators">
        /// A collection of <see cref="IValidator{TCommand}"/>s registered for 
        /// the specific <typeparamref name="TRequest"/>.
        /// </param>
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        /// <inheritdoc/>
        async ValueTask<TResult> IPipelineBehavior<TRequest, TResult>.HandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            // If no validators are registered for this command
            if (_validators.Any() is false)
            {
                // Continue the pipeline
                return await next();
            }

            // Get a collection of validation errors from the collection of validators
            List<ValidationError> errors = _validators
                // Select each validator's collection of validation errors
                .SelectMany((IValidator<TRequest> validator) => validator.Validate(request))
                // Output to a list
                .ToList();

            // If validation errors occurred
            if (errors.Count != 0)
            {
                // Throw a validation exception
                throw new ValidationException(errors);
            }

            // Continue the pipeline
            return await next();
        }
    }
}
