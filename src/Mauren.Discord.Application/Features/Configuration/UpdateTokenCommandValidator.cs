using Mauren.Discord.Application.Abstractions.Validation;
using System;
using System.Collections.Generic;

namespace Mauren.Discord.Application.Features.Configuration
{
    /// <inheritdoc/>
    internal class UpdateTokenCommandValidator : IValidator<UpdateTokenCommand>
    {
        /// <inheritdoc/>
        IEnumerable<ValidationError> IValidator<UpdateTokenCommand>.Validate(UpdateTokenCommand command)
        {
            if (String.IsNullOrWhiteSpace(command.NewToken))
            {
                yield return new ValidationError(nameof(command.NewToken), "The bot token cannot be empty.");
            }
            else if (command.NewToken.Length < 50)
            {
                yield return new ValidationError(nameof(command.NewToken), "The bot token is too short to be a valid Discord token.");
            }
        }
    }
}
