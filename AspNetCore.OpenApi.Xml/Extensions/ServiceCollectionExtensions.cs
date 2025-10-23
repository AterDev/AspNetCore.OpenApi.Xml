using AspNetCore.OpenApi.Xml.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.OpenApi.Xml.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiDocument(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer(); // ensure ApiExplorer is enabled
        services.AddSingleton<IXmlDocumentationReader, XmlDocumentationReader>();
        services.AddSingleton<IApiXmlDocumentGenerator, ApiXmlDocumentGenerator>();
        
        // Add Razor Components support for the UI
        services.AddRazorComponents();
        
        return services;
    }
}
