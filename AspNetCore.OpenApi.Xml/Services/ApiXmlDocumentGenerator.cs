using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OpenApi.Xml.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.OpenApi.Xml.Services;

public interface IApiXmlDocumentGenerator
{
    ApiDocument Generate(string? title = null, string? version = null);
    string GenerateXml(string? title = null, string? version = null, bool indented = true, string? encodingName = "utf-8");
}

public class ApiXmlDocumentGenerator(IApiDescriptionGroupCollectionProvider provider) : IApiXmlDocumentGenerator
{
    public ApiDocument Generate(string? title = null, string? version = null)
    {
        var doc = new ApiDocument
        {
            Title = title ?? "API Documentation",
            Version = version ?? "1.0"
        };

        foreach (var group in provider.ApiDescriptionGroups.Items)
        {
            foreach (var api in group.Items)
            {
                var endpoint = new Endpoint
                {
                    Path = api.RelativePath ?? string.Empty,
                    Method = api.HttpMethod ?? "GET",
                    Summary = api.ActionDescriptor?.DisplayName,
                    Description = api.ActionDescriptor?.RouteValues.TryGetValue("action", out var actionName) == true ? actionName : null,
                    Deprecated = api.ActionDescriptor?.EndpointMetadata.OfType<ObsoleteAttribute>().Any() == true,
                    Tags = group.GroupName != null ? new List<string> { group.GroupName } : null
                };

                var request = new ApiRequest();

                // Parameters (route/query/header + body)
                foreach (var param in api.ParameterDescriptions)
                {
                    if (param.Source == BindingSource.Body)
                    {
                        // Handle later as body
                        continue;
                    }

                    var field = new ApiField
                    {
                        Name = param.Name,
                        Type = ResolveSimpleType(param.Type),
                        Required = param.IsRequired,
                        Description = param.ModelMetadata?.Description
                    };

                    if (param.Source == BindingSource.Path)
                    {
                        request.RouteParameters.Add(field);
                    }
                    else if (param.Source == BindingSource.Query)
                    {
                        request.QueryParameters.Add(field);
                    }
                    else if (param.Source == BindingSource.Header)
                    {
                        request.Headers.Add(field);
                    }
                }

                // Request body (first body parameter)
                var bodyParameter = api.ParameterDescriptions.FirstOrDefault(p => p.Source == BindingSource.Body);
                if (bodyParameter?.Type != null && bodyParameter.Type != typeof(void))
                {
                    request.Body = BuildSchema(bodyParameter.Type);
                }

                endpoint.Request = request;

                // Responses
                foreach (var responseType in api.SupportedResponseTypes)
                {
                    var resp = new ApiResponse
                    {
                        StatusCode = responseType.StatusCode,
                        ContentType = responseType.ApiResponseFormats.FirstOrDefault()?.MediaType
                    };

                    if (responseType.Type != null && responseType.Type != typeof(void))
                    {
                        resp.Body = BuildSchema(responseType.Type);
                    }

                    endpoint.Responses.Add(resp);
                }

                doc.Endpoints.Add(endpoint);
            }
        }

        return doc;
    }

    public string GenerateXml(string? title = null, string? version = null, bool indented = true, string? encodingName = "utf-8")
    {
        var document = Generate(title, version);
        var serializer = new XmlSerializer(typeof(ApiDocument));
        var settings = new XmlWriterSettings
        {
            Indent = indented,
            Encoding = Encoding.GetEncoding(encodingName ?? "utf-8"),
            OmitXmlDeclaration = false
        };
        using var sw = new StringWriterWithEncoding(settings.Encoding);
        using (var writer = XmlWriter.Create(sw, settings))
        {
            serializer.Serialize(writer, document);
        }
        return sw.ToString();
    }

    private static string ResolveSimpleType(Type? type)
    {
        if (type == null) return "object";
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type.IsEnum) return "enum";
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short)) return "integer";
        if (type == typeof(decimal) || type == typeof(float) || type == typeof(double)) return "number";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(DateTime) || type == typeof(DateOnly) || type == typeof(TimeOnly) || type == typeof(TimeSpan)) return "string";
        if (type == typeof(Guid)) return "string";
        if (type.IsArray) return "array";
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string)) return "array";
        return "object";
    }

    private static ApiSchema BuildSchema(Type type, int depth = 0)
    {
        const int maxDepth = 8; // Prevent runaway recursion
        if (depth > maxDepth)
        {
            return new ApiSchema { Type = "object", Description = "Max depth exceeded" };
        }

        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        var schemaType = ResolveSimpleType(underlying);
        var schema = new ApiSchema
        {
            Type = schemaType,
            Nullable = underlying != type
        };

        if (schemaType == "array")
        {
            Type elementType = typeof(object);
            if (underlying.IsArray)
            {
                elementType = underlying.GetElementType() ?? typeof(object);
            }
            else
            {
                var enumerable = underlying.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (enumerable != null)
                {
                    elementType = enumerable.GetGenericArguments()[0];
                }
            }
            schema.ArrayItem = BuildSchema(elementType, depth + 1);
            return schema;
        }

        if (schemaType == "object" && !underlying.IsPrimitive && underlying != typeof(string))
        {
            foreach (var prop in underlying.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.GetMethod == null || prop.GetMethod.GetParameters().Length > 0) continue;
                var field = new ApiField
                {
                    Name = prop.Name,
                    Type = ResolveSimpleType(prop.PropertyType),
                    Required = IsRequired(prop),
                    Description = GetDescription(prop)
                };
                schema.Fields.Add(field);
            }
        }

        return schema;
    }

    private static bool IsRequired(PropertyInfo prop)
    {
        // DataAnnotations, minimal set
        return Attribute.IsDefined(prop, typeof(RequiredAttribute));
    }

    private static string? GetDescription(MemberInfo member)
    {
        var descAttr = member.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return descAttr?.Description;
    }

    private sealed class StringWriterWithEncoding : StringWriter
    {
        public override Encoding Encoding { get; }
        public StringWriterWithEncoding(Encoding encoding) => Encoding = encoding;
    }
}
