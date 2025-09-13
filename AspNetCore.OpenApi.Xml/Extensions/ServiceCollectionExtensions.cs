using AspNetCore.OpenApi.Xml.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.OpenApi.Xml.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiXmlDocumentGenerator(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer(); // ensure ApiExplorer is enabled
        services.AddSingleton<IApiXmlDocumentGenerator, ApiXmlDocumentGenerator>();
        return services;
    }
}
