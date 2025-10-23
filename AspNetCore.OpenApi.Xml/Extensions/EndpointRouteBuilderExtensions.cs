using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AspNetCore.OpenApi.Xml.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the API documentation page at the default route ("/api-doc").
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapApiDocumentationPage(this IEndpointRouteBuilder endpoints)
    {
        // Map Razor Pages (which includes our ApiDocumentation page at /api-doc)
        endpoints.MapRazorPages();
        
        return endpoints;
    }
}
