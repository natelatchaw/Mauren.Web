using System;

namespace Mauren.Discord.Application.Abstractions.Validation
{
    /// <summary>
    /// Represents a single validation failure.
    /// </summary>
    public record ValidationError(String PropertyName, String ErrorMessage);
}
