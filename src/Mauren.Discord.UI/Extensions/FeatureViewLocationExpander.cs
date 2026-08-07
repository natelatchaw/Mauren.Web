using Microsoft.AspNetCore.Mvc.Controllers;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    public class FeatureViewLocationExpander : IViewLocationExpander
    {
        /// <inheritdoc/>
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // If the current request is bound to an MVC Controller
            if (context.ActionContext.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                // 
                if (descriptor.Properties.TryGetValue("feature", out Object? featureProperty) && featureProperty is String featureNameProperty)
                {
                    context.Values["feature"] = featureNameProperty;
                }
                //
                else
                {
                    // Extract the feature name from the end of the namespace
                    String? featureName = descriptor.ControllerTypeInfo.Namespace?.Split('.').Last();

                    // If the feature name is not null/empty
                    if (String.IsNullOrEmpty(featureName) is false)
                    {
                        // Store the feature name in the context so ExpandViewLocations can use it
                        context.Values["feature"] = featureName;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<String> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<String> viewLocations)
        {
            List<String> locations = [];

            // Try to get the value for the 'feature' key in the current context's values
            if (context.Values.TryGetValue("feature", out String? featureName) && String.IsNullOrEmpty(featureName) is false)
            {
                locations.Add($"/Features/{featureName}/{{0}}.cshtml");
                locations.Add($"/Features/{featureName}/Views/{{0}}.cshtml");
            }

            // Add Global Shared Feature locations
            locations.Add("/Features/Shared/{0}.cshtml");
            locations.Add("/Features/Shared/Views/{0}.cshtml");
            locations.Add("/Views/Shared/{0}.cshtml");

            // Return the feature locations combined with the default ASP.NET Core locations
            return locations.Concat(viewLocations);
        }
    }
}
