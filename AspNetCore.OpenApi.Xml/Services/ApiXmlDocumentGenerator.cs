using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OpenApi.Xml.Core.Models;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AspNetCore.OpenApi.Xml.Services;

public interface IApiXmlDocumentGenerator
{
    ApiDocument Generate(string? title = null, string? version = null);
    string GenerateXml(string? title = null, string? version = null, bool indented = true, string? encodingName = "utf-8");
}

public class ApiXmlDocumentGenerator(IApiDescriptionGroupCollectionProvider provider, IXmlDocumentationReader xmlDocReader) : IApiXmlDocumentGenerator
{
    private readonly Dictionary<Type, ApiModel> _modelCache = [];
    private static readonly HashSet<Type> _primitiveTypes = new(
        new[]
        {
            typeof(string), typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
            typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(Guid), typeof(DateTime),
            typeof(DateOnly), typeof(TimeOnly), typeof(TimeSpan)
        });

    private ApiDocument? _cachedDocument;
    private bool _xmlDocumentationLoaded;

    public ApiDocument Generate(string? title = null, string? version = null)
    {
        // Return cached document if already generated with same parameters
        if (_cachedDocument != null && 
            _cachedDocument.Title == (title ?? "API Documentation") && 
            _cachedDocument.Version == (version ?? "1.0"))
        {
            return _cachedDocument;
        }

        _modelCache.Clear();

        // Load XML documentation only once
        if (!_xmlDocumentationLoaded)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    xmlDocReader.LoadXmlDocumentation(assembly);
                }
                catch
                {
                }
            }
            _xmlDocumentationLoaded = true;
        }

        var doc = new ApiDocument { Title = title ?? "API Documentation", Version = version ?? "1.0" };
        int opIndex = 0;

        foreach (var group in provider.ApiDescriptionGroups.Items)
        {
            foreach (var api in group.Items)
            {
                var operationId = BuildOperationId(api, ++opIndex);
                var tags = new List<string>();
                if (!string.IsNullOrWhiteSpace(group.GroupName)) tags.Add(group.GroupName!);
                if (api.ActionDescriptor?.RouteValues.TryGetValue("controller", out var controllerName) == true && !string.IsNullOrWhiteSpace(controllerName) && !tags.Contains(controllerName))
                    tags.Add(controllerName);

                // Try to get XML documentation summary
                string? summary = null;
                if (api.ActionDescriptor is ControllerActionDescriptor controllerAction)
                {
                    summary = xmlDocReader.GetMethodSummary(controllerAction.MethodInfo);
                }

                var endpoint = new Endpoint
                {
                    OperationId = operationId,
                    Path = api.RelativePath ?? string.Empty,
                    Method = api.HttpMethod ?? "GET",
                    Summary = summary ?? api.ActionDescriptor?.DisplayName,
                    Description = api.ActionDescriptor?.RouteValues.TryGetValue("action", out var actionName) == true ? actionName : null,
                    Deprecated = api.ActionDescriptor?.EndpointMetadata.OfType<ObsoleteAttribute>().Any() == true,
                    Tags = tags.Count > 0 ? tags : null
                };

                var request = new ApiRequest();
                foreach (var param in api.ParameterDescriptions)
                {
                    if (param.Source == BindingSource.Body) continue;
                    var field = new ApiField
                    {
                        Name = param.Name,
                        Type = MapPrimitiveName(param.Type),
                        Required = param.IsRequired,
                        Description = param.ModelMetadata?.Description
                    };
                    // validation
                    foreach (var a in GetParameterAttributes(param))
                        ApplyValidation(field, a);

                    if (NeedsModelReference(param.Type))
                        field.ModelId = GetOrBuildModel(param.Type).Id;

                    if (param.Source == BindingSource.Path) request.RouteParameters.Add(field);
                    else if (param.Source == BindingSource.Query) request.QueryParameters.Add(field);
                    else if (param.Source == BindingSource.Header) request.Headers.Add(field);
                }
                var bodyParam = api.ParameterDescriptions.FirstOrDefault(p => p.Source == BindingSource.Body);
                if (bodyParam?.Type != null && bodyParam.Type != typeof(void) && NeedsModelReference(bodyParam.Type))
                {
                    request.Body = GetOrBuildModel(bodyParam.Type);
                }
                endpoint.Request = request;

                foreach (var resp in api.SupportedResponseTypes)
                {
                    var response = new ApiResponse
                    {
                        StatusCode = resp.StatusCode,
                        ContentType = resp.ApiResponseFormats.FirstOrDefault()?.MediaType ?? "application/json",
                        Body = resp.Type != null && resp.Type != typeof(void) && NeedsModelReference(resp.Type)
                            ? GetOrBuildModel(resp.Type)
                            : null
                    };
                    endpoint.Responses.Add(response);
                }

                // If no responses were defined, add a default 200 response with application/json
                if (endpoint.Responses.Count == 0)
                {
                    endpoint.Responses.Add(new ApiResponse
                    {
                        StatusCode = 200,
                        ContentType = ""
                    });
                }

                doc.Endpoints.Add(endpoint);
            }
        }

        doc.Models.AddRange(_modelCache.Values
            .Where(m => m.ModelType != ModelType.Primitive && !(m.ModelType == ModelType.Array && m.ElementType != null && m.ElementType.ModelType == ModelType.Primitive))
            .OrderBy(m => m.Id));
        
        // Cache the generated document
        _cachedDocument = doc;
        return doc;
    }

    public string GenerateXml(string? title = null, string? version = null, bool indented = true, string? encodingName = "utf-8")
    {
        var document = Generate(title, version);
        var serializer = new XmlSerializer(typeof(ApiDocument));
        var settings = new XmlWriterSettings { Indent = indented, Encoding = Encoding.GetEncoding(encodingName ?? "utf-8"), OmitXmlDeclaration = false };
        using var sw = new StringWriterWithEncoding(settings.Encoding);
        using var writer = XmlWriter.Create(sw, settings); serializer.Serialize(writer, document); return sw.ToString();
    }

    private static IEnumerable<Attribute> GetParameterAttributes(ApiParameterDescription param)
    {
        return param.ParameterDescriptor is ControllerParameterDescriptor cpd
            ? cpd.ParameterInfo.GetCustomAttributes()
            : Enumerable.Empty<Attribute>();
    }

    private static bool NeedsModelReference(Type? type)
    {
        if (type == null) return false;
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (_primitiveTypes.Contains(type)) return false;
        if (type.IsEnum) return true; // we keep enum definitions
        if (type.IsArray)
        {
            var et = type.GetElementType();
            return et != null && NeedsModelReference(et);
        }
        return !IsSimpleEnumerablePrimitive(type);
    }

    private ApiModel GetOrBuildModel(Type type, int depth = 0)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return BuildModel(type, depth);
    }

    private ApiModel BuildModel(Type type, int depth = 0)
    {
        if (_modelCache.TryGetValue(type, out var existing)) return existing;
        const int maxDepth = 12;
        var nullableUnderlying = Nullable.GetUnderlyingType(type);
        var coreType = nullableUnderlying ?? type;
        var id = BuildModelId(coreType);

        var model = new ApiModel
        {
            Id = id,
            Name = coreType.IsGenericType ? coreType.Name.Split('`')[0] : coreType.Name,
            Namespace = coreType.Namespace,
            Nullable = nullableUnderlying != null,
            ModelType = ModelType.Custom
        };
        _modelCache[coreType] = model;

        if (depth > maxDepth)
        {
            model.Description = "Max depth exceeded";
            return model;
        }

        if (coreType.IsEnum)
        {
            model.ModelType = ModelType.Enum;
            foreach (var f in coreType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var desc = f.GetCustomAttribute<DescriptionAttribute>()?.Description;
                var value = Convert.ChangeType(f.GetRawConstantValue(), Enum.GetUnderlyingType(coreType))?.ToString() ?? "";
                model.EnumMembers.Add(new EnumMember { Name = f.Name, Value = value, Description = desc });
            }
            return model;
        }

        if (IsPrimitiveLike(coreType))
        {
            model.ModelType = ModelType.Primitive;
            model.Name = MapPrimitiveName(coreType, false);
            model.Id = MapPrimitiveName(coreType, true);
            return model; // will be filtered out later
        }

        if (coreType.IsArray)
        {
            model.ModelType = ModelType.Array;
            model.ArrayRank = coreType.GetArrayRank();
            var et = coreType.GetElementType();
            if (et != null && NeedsModelReference(et))
                model.ElementType = GetOrBuildModel(et, depth + 1);
            else if (et != null)
                model.ElementType = new ApiModel { Id = MapPrimitiveName(et, true), Name = MapPrimitiveName(et, false), ModelType = ModelType.Primitive };
            return model;
        }

        if (IsDictionaryType(coreType))
        {
            model.ModelType = ModelType.Dictionary;
            var iface = coreType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            var args = iface.GetGenericArguments();
            model.KeyType = NeedsModelReference(args[0]) ? GetOrBuildModel(args[0], depth + 1) : new ApiModel { Id = MapPrimitiveName(args[0], true), Name = MapPrimitiveName(args[0], false), ModelType = ModelType.Primitive };
            model.ValueType = NeedsModelReference(args[1]) ? GetOrBuildModel(args[1], depth + 1) : new ApiModel { Id = MapPrimitiveName(args[1], true), Name = MapPrimitiveName(args[1], false), ModelType = ModelType.Primitive };
            return model;
        }

        if (IsTupleType(coreType))
        {
            model.ModelType = ModelType.Tuple;
            int i = 1;
            foreach (var arg in coreType.GetGenericArguments())
            {
                var needs = NeedsModelReference(arg);
                model.TupleElements.Add(new ApiField
                {
                    Name = $"Item{i++}",
                    Type = MapPrimitiveName(arg),
                    Required = true,
                    ModelId = needs ? GetOrBuildModel(arg, depth + 1).Id : null
                });
            }
            return model;
        }

        if (coreType.IsGenericType)
        {
            model.IsOpenGeneric = coreType.IsGenericTypeDefinition;
            foreach (var ga in coreType.GetGenericArguments())
            {
                if (NeedsModelReference(ga))
                    model.GenericArguments.Add(GetOrBuildModel(ga, depth + 1));
            }
            if (typeof(IEnumerable).IsAssignableFrom(coreType))
            {
                var element = TryGetEnumerableElementType(coreType);
                if (element != null)
                {
                    model.ModelType = ModelType.Array;
                    model.ElementType = NeedsModelReference(element) ? GetOrBuildModel(element, depth + 1) : new ApiModel { Id = MapPrimitiveName(element, true), Name = MapPrimitiveName(element, false), ModelType = ModelType.Primitive };
                    return model;
                }
            }
            if (model.ModelType == ModelType.Custom)
                model.ModelType = ModelType.Generic;
        }

        // Object / Custom
        if (model.ModelType is ModelType.Custom or ModelType.Object)
        {
            model.ModelType = ModelType.Object;
            foreach (var prop in coreType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetMethod == null || prop.GetMethod.GetParameters().Length > 0) continue;
                var field = new ApiField
                {
                    Name = prop.Name,
                    Type = MapPrimitiveName(prop.PropertyType),
                    Required = IsRequired(prop)
                };
                foreach (var a in prop.GetCustomAttributes())
                    ApplyValidation(field, a);
                if (NeedsModelReference(prop.PropertyType))
                    field.ModelId = GetOrBuildModel(prop.PropertyType, depth + 1).Id;
                model.Fields.Add(field);
            }
            if (model.Fields.Count == 0) model.ModelType = ModelType.Custom;
        }
        return model;
    }

    private static void ApplyValidation(ApiField field, Attribute attr)
    {
        switch (attr)
        {
            case StringLengthAttribute s:
                if (s.MinimumLength > 0) field.MinLength = s.MinimumLength;
                if (s.MaximumLength > 0) field.MaxLength = s.MaximumLength;
                break;
            case MinLengthAttribute min:
                field.MinLength = min.Length; break;
            case MaxLengthAttribute max:
                field.MaxLength = max.Length; break;
            case RegularExpressionAttribute regex:
                field.Pattern = regex.Pattern; break;
            case RangeAttribute range:
                field.Minimum = range.Minimum?.ToString();
                field.Maximum = range.Maximum?.ToString();
                break;
        }
    }

    private static bool IsSimpleEnumerablePrimitive(Type t)
    {
        if (t == typeof(string)) return false;
        if (!typeof(IEnumerable).IsAssignableFrom(t)) return false;
        var elem = TryGetEnumerableElementType(t);
        return elem != null && _primitiveTypes.Contains(elem);
    }

    private static string BuildModelId(Type type)
    {
        if (IsPrimitiveLike(type)) return MapPrimitiveName(type, true);
        if (type.IsGenericType)
        {
            var name = type.Name.Split('`')[0];
            var args = type.GetGenericArguments().Select(a => BuildModelId(a).Split('.').Last());
            return $"{type.Namespace}.{name}Of{string.Join("And", args)}";
        }
        if (type.IsArray)
        {
            var et = type.GetElementType();
            return et != null ? $"ArrayOf{BuildModelId(et).Split('.').Last()}" : "Array";
        }
        return $"{type.Namespace}.{type.Name}";
    }

    private static string MapPrimitiveName(Type? t, bool includeNamespace = false)
    {
        if (t == null) return "object";
        t = Nullable.GetUnderlyingType(t) ?? t;

        // Handle generic types - display with type parameters
        if (t.IsGenericType)
        {
            var baseName = t.Name.Split('`')[0];
            var args = t.GetGenericArguments();
            var argNames = string.Join(", ", args.Select(a => MapPrimitiveName(a, false)));
            var typeName = $"{baseName}<{argNames}>";
            return includeNamespace ? $"{t.Namespace}.{typeName}" : typeName;
        }

        return t switch
        {
            var _ when t == typeof(string) => includeNamespace ? "System.string" : "string",
            var _ when t == typeof(bool) => includeNamespace ? "System.boolean" : "boolean",
            var _ when t == typeof(int) => includeNamespace ? "System.int32" : "int32",
            var _ when t == typeof(long) => includeNamespace ? "System.int64" : "int64",
            var _ when t == typeof(short) => includeNamespace ? "System.int16" : "int16",
            var _ when t == typeof(uint) => includeNamespace ? "System.uint32" : "uint32",
            var _ when t == typeof(ulong) => includeNamespace ? "System.uint64" : "uint64",
            var _ when t == typeof(ushort) => includeNamespace ? "System.uint16" : "uint16",
            var _ when t == typeof(byte) => includeNamespace ? "System.byte" : "byte",
            var _ when t == typeof(sbyte) => includeNamespace ? "System.sbyte" : "sbyte",
            var _ when t == typeof(float) => includeNamespace ? "System.float" : "float",
            var _ when t == typeof(double) => includeNamespace ? "System.double" : "double",
            var _ when t == typeof(decimal) => includeNamespace ? "System.decimal" : "decimal",
            var _ when t == typeof(Guid) => includeNamespace ? "System.guid" : "guid",
            var _ when t == typeof(DateTime) => includeNamespace ? "System.datetime" : "datetime",
            var _ when t == typeof(DateOnly) => includeNamespace ? "System.date" : "date",
            var _ when t == typeof(TimeOnly) => includeNamespace ? "System.time" : "time",
            var _ when t == typeof(TimeSpan) => includeNamespace ? "System.duration" : "duration",
            _ => includeNamespace ? (t.Namespace + "." + t.Name) : t.Name
        };
    }

    private static bool IsPrimitiveLike(Type t) => _primitiveTypes.Contains(t);

    private static Type? TryGetEnumerableElementType(Type type)
    {
        if (type.IsArray) return type.GetElementType();
        var enumerable = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return enumerable?.GetGenericArguments()[0];
    }

    private static bool IsDictionaryType(Type t)
        => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

    private static bool IsTupleType(Type t)
        => (t.FullName?.StartsWith("System.ValueTuple") ?? false) || (t.IsGenericType && t.FullName!.StartsWith("System.Tuple"));

    private string BuildOperationId(ApiDescription api, int index)
    {
        var route = api.RelativePath ?? ($"op{index}");
        var method = api.HttpMethod ?? "GET";
        var cleanRoute = new string(route.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        return $"{method}_{cleanRoute}".Trim('_');
    }

    private static bool IsRequired(PropertyInfo prop)
        => Attribute.IsDefined(prop, typeof(RequiredAttribute));

    private sealed class StringWriterWithEncoding : StringWriter
    {
        public override Encoding Encoding { get; }
        public StringWriterWithEncoding(Encoding encoding) => Encoding = encoding;
    }
}
