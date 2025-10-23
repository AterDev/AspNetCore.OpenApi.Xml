using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AspNetCore.OpenApi.Xml.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the API documentation page at the specified route (default: "/api-doc").
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern for the API documentation page (e.g. "/api-doc").</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    /// <remarks>
    /// The <c>title</c> and <c>version</c> of the API documentation can be provided via query string parameters,
    /// for example: <c>/api-doc?title=MyAPI&amp;version=2.0</c>.
    /// </remarks>
    public static IEndpointRouteBuilder MapApiDocument(this IEndpointRouteBuilder endpoints, string pattern = "/api-doc")
    {
        // Map the Blazor app which includes all Razor components
        // This needs to be called once for the entire application, not per-pattern
        endpoints.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();

        return endpoints;
    }
}
