using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;

namespace Mauren.Discord.UI.Extensions
{
    public class FeatureConvention : IControllerModelConvention
    {
        /// <inheritdoc/>
        void IControllerModelConvention.Apply(ControllerModel controller)
        {
            // Get the name of the feature the controller type belongs to
            String value = GetFeatureName(controller.ControllerType);
            // Add the feature name to the controller properties
            controller.Properties.Add("feature", value);
        }

        private String GetFeatureName(TypeInfo controllerType)
        {
            // Split the controller's full name by segments
            IEnumerable<String> tokens = controllerType.FullName?.Split('.') ?? Enumerable.Empty<String>();

            // Determine whether the controller's name has any segments matching 'Features'
            Boolean hasFeatureToken = tokens.Any((String token) => token.Equals("Features", StringComparison.OrdinalIgnoreCase));
            // If the controller's full name does not have a 'Feature' segment, return empty string
            if (hasFeatureToken is false) return String.Empty;

            // Get the name of the feature
            String featureName = tokens
                // Skip tokens until reaching the token that starts with 'Features'
                .SkipWhile((String token) => token.Equals("Features", StringComparison.CurrentCultureIgnoreCase) is false)
                // Skip the 'Features' token
                .Skip(1)
                // Get the token immediately after
                .FirstOrDefault(String.Empty);

            // Return the feature name
            return featureName;
        }
    }
}
