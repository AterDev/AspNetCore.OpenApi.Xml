using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using AspNetCore.OpenApi.Xml.Services;

namespace AspNetCore.OpenApi.Xml.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the API documentation page at the specified pattern.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern (default: "/api-doc").</param>
    /// <param name="title">The title of the documentation (optional).</param>
    /// <param name="version">The version of the documentation (optional).</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapApiDocumentationPage(
        this IEndpointRouteBuilder endpoints, 
        string pattern = "/api-doc",
        string? title = null,
        string? version = null)
    {
        endpoints.MapGet(pattern, (IApiXmlDocumentGenerator generator, IApiDocumentationPageService pageService) =>
        {
            var document = generator.Generate(title ?? "API Documentation", version ?? "1.0");
            var html = pageService.GenerateHtml(document);
            return Results.Content(html, "text/html; charset=utf-8", Encoding.UTF8);
        });
        
        return endpoints;
    }
}
