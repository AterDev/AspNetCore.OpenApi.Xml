using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenApi.Xml.Core.Models;

/// <summary>
/// Root of the custom API XML document describing all discovered HTTP endpoints.
/// </summary>
[XmlRoot("ApiDocument")]
public class ApiDocument
{
    /// <summary>
    /// Document title (logical name of the API set).
    /// </summary>
    [XmlAttribute("title")] public string Title { get; set; } = "API Documentation";

    /// <summary>
    /// Version of the document (not necessarily the assembly version).
    /// </summary>
    [XmlAttribute("version")] public string Version { get; set; } = "1.0";

    /// <summary>
    /// Collection of API endpoints (operations).
    /// </summary>
    [XmlElement("Endpoint")] public List<Endpoint> Endpoints { get; set; } = [];

    /// <summary>Centralized reusable model definitions (similar to OpenAPI components.schemas).</summary>
    [XmlArray("Models"), XmlArrayItem("Model")] public List<ApiModel> Models { get; set; } = [];
}

/// <summary>Represents a single HTTP endpoint (operation) with request and response metadata.</summary>
public class Endpoint
{
    /// <summary>
    /// Unique operation identifier (operationId) across the whole document.
    /// </summary>
    [XmlAttribute("operationId")] public required string OperationId { get; set; }

    /// <summary>
    /// The route template (relative path) e.g. "users/{id}".
    /// </summary>
    [XmlAttribute("path")] public required string Path { get; set; }

    /// <summary>
    /// The HTTP method verb (GET / POST / PUT / DELETE ...).
    /// </summary>
    [XmlAttribute("method")] public required string Method { get; set; }

    /// <summary>
    /// Indicates the endpoint is obsolete / deprecated.
    /// </summary>
    [XmlAttribute("deprecated")] public bool Deprecated { get; set; }

    /// <summary>
    /// Short one‑line summary.
    /// </summary>
    [XmlElement("Summary")] public string? Summary { get; set; }

    /// <summary>
    /// Longer textual description.
    /// </summary>
    [XmlElement("Description")] public string? Description { get; set; }

    /// <summary>
    /// Free form classification tags.
    /// </summary>
    [XmlArray("Tags"), XmlArrayItem("Tag")] public List<string>? Tags { get; set; }

    /// <summary>
    /// Request metadata (parameters + body).
    /// </summary>
    [XmlElement("Request")] public ApiRequest? Request { get; set; }

    /// <summary>
    /// Possible responses for this endpoint.
    /// </summary>
    [XmlArray("Responses"), XmlArrayItem("Response")] public List<ApiResponse> Responses { get; set; } = [];
}

/// <summary>HTTP request definition including route, query, header parameters and optional body model.</summary>
public class ApiRequest
{
    /// <summary>
    /// Header parameters.
    /// </summary>
    [XmlElement("Header")] public List<ApiField> Headers { get; set; } = [];

    /// <summary>
    /// Query string parameters.
    /// </summary>
    [XmlElement("Query")] public List<ApiField> QueryParameters { get; set; } = [];

    /// <summary>
    /// Route (path) parameters appearing inside the template.
    /// </summary>
    [XmlElement("Route")] public List<ApiField> RouteParameters { get; set; } = [];

    /// <summary>
    /// Optional request body schema definition.
    /// </summary>
    [XmlElement("Body")] public ApiModel? Body { get; set; }

    /// <summary>
    /// Additional descriptive remarks for the request.
    /// </summary>
    [XmlElement("Description")] public string? Description { get; set; }
}

/// <summary>HTTP response definition including status code, content type and payload model.</summary>
public class ApiResponse
{
    /// <summary>
    /// HTTP status code.
    /// </summary>
    [XmlAttribute("status")] public int StatusCode { get; set; }

    /// <summary>
    /// Primary content type (first negotiated media type).
    /// </summary>
    [XmlAttribute("contentType")] public string? ContentType { get; set; }

    /// <summary>
    /// Human readable description for the response.
    /// </summary>
    [XmlElement("Description")] public string? Description { get; set; }

    /// <summary>
    /// Optional body schema for the payload.
    /// </summary>
    [XmlElement("Body")] public ApiModel? Body { get; set; }
}

/// <summary>Parameter / property / tuple element definition referencing a concrete <see cref="ApiModel"/>.</summary>
public class ApiField
{
    /// <summary>
    /// Logical name (property / parameter / element).
    /// </summary>
    [XmlAttribute("name")] public required string Name { get; set; }

    /// <summary>
    /// Primitive/simple type hint (string, integer, number, boolean, enum, object, array ...).
    /// </summary>
    [XmlAttribute("type")] public string? Type { get; set; }

    /// <summary>
    /// True if value is required (e.g. non-nullable or annotated).
    /// </summary>
    [XmlAttribute("required")] public bool Required { get; set; }

    /// <summary>
    /// Default value (string representation) if any.
    /// </summary>
    [XmlAttribute("default")] public string? DefaultValue { get; set; }

    /// <summary>
    /// Human readable description.
    /// </summary>
    [XmlElement("Description")] public string? Description { get; set; }

    /// <summary>
    /// An example value (stringified) for documentation.
    /// </summary>
    [XmlElement("Example")] public string? Example { get; set; }

    /// <summary>
    /// Full schema for complex / structured types (optional for primitives).
    /// </summary>
    [XmlAttribute("modelId")] public string? ModelId { get; set; } // reference to complex model

    // Validation metadata (from attributes / annotations)
    [XmlAttribute("minLength")] public int? MinLength { get; set; }
    [XmlAttribute("maxLength")] public int? MaxLength { get; set; }
    [XmlAttribute("pattern")] public string? Pattern { get; set; }
    [XmlAttribute("minimum")] public string? Minimum { get; set; }
    [XmlAttribute("maximum")] public string? Maximum { get; set; }

    public bool ShouldSerializeMinLength() => MinLength.HasValue;
    public bool ShouldSerializeMaxLength() => MaxLength.HasValue;
    public bool ShouldSerializePattern() => !string.IsNullOrWhiteSpace(Pattern);
    public bool ShouldSerializeMinimum() => Minimum != null;
    public bool ShouldSerializeMaximum() => Maximum != null;
    public bool ShouldSerializeModelId() => !string.IsNullOrWhiteSpace(ModelId);
}

/// <summary>High level classification of a model to support cross language mapping.</summary>
public enum ModelType
{
    Primitive,
    Enum,
    Object,
    Array,
    Dictionary,
    Generic,
    Tuple,
    Union,
    Custom
}

/// <summary>
/// Detailed structural & semantic description of a type (reflection-like). When used as a reference
/// (Body / Field inline) only the Id is required.
/// </summary>
public class ApiModel
{
    /// <summary>Unique identifier for the model (namespace + friendly name).</summary>
    [XmlAttribute("id")] public string? Id { get; set; }

    /// <summary>Classification (primitive / object / array / dictionary / generic / tuple / union / custom).</summary>
    [XmlAttribute("modelType")] public ModelType ModelType { get; set; } = ModelType.Custom;

    /// <summary>Short (unqualified) name.</summary>
    [XmlAttribute("name")] public string? Name { get; set; }

    /// <summary>Namespace portion.</summary>
    [XmlAttribute("namespace")] public string? Namespace { get; set; }

    /// <summary>Indicates nullable.</summary>
    [XmlAttribute("nullable")] public bool Nullable { get; set; }

    /// <summary>Array rank if an array (0 otherwise).</summary>
    [XmlAttribute("arrayRank")] public int ArrayRank { get; set; }

    /// <summary>True if open generic definition.</summary>
    [XmlAttribute("openGeneric")] public bool IsOpenGeneric { get; set; }

    /// <summary>Primitive/data format hint (date-time, uuid ...).</summary>
    [XmlElement("Format")] public string? Format { get; set; }

    /// <summary>Description or summary.</summary>
    [XmlElement("Description")] public string? Description { get; set; }

    [XmlArray("Fields"), XmlArrayItem("Field")] public List<ApiField> Fields { get; set; } = [];
    [XmlElement("ElementType")] public ApiModel? ElementType { get; set; }
    [XmlElement("KeyType")] public ApiModel? KeyType { get; set; }
    [XmlElement("ValueType")] public ApiModel? ValueType { get; set; }
    [XmlArray("GenericArguments"), XmlArrayItem("TypeArg")] public List<ApiModel> GenericArguments { get; set; } = [];
    [XmlElement("UnderlyingType")] public ApiModel? UnderlyingType { get; set; }
    [XmlArray("TupleElements"), XmlArrayItem("Element")] public List<ApiField> TupleElements { get; set; } = [];
    [XmlArray("UnionTypes"), XmlArrayItem("Union")] public List<ApiModel> UnionTypes { get; set; } = [];

    // Enum members with optional description
    [XmlArray("EnumMembers"), XmlArrayItem("Member")] public List<EnumMember> EnumMembers { get; set; } = [];

    public bool ShouldSerializeFields() => Fields.Count > 0;
    public bool ShouldSerializeElementType() => ElementType?.Id != null && ElementType.ModelType != ModelType.Primitive;
    public bool ShouldSerializeKeyType() => KeyType?.Id != null;
    public bool ShouldSerializeValueType() => ValueType?.Id != null;
    public bool ShouldSerializeGenericArguments() => GenericArguments.Count > 0;
    public bool ShouldSerializeUnderlyingType() => UnderlyingType?.Id != null;
    public bool ShouldSerializeTupleElements() => TupleElements.Count > 0;
    public bool ShouldSerializeUnionTypes() => UnionTypes.Count > 0;
    public bool ShouldSerializeEnumMembers() => EnumMembers.Count > 0;
    public bool ShouldSerializeArrayRank() => ArrayRank > 0;
    public bool ShouldSerializeIsOpenGeneric() => IsOpenGeneric;
    public bool ShouldSerializeFormat() => !string.IsNullOrWhiteSpace(Format);
    public bool ShouldSerializeDescription() => !string.IsNullOrWhiteSpace(Description);
    public bool ShouldSerializeName() => !string.IsNullOrWhiteSpace(Name);
    public bool ShouldSerializeNamespace() => !string.IsNullOrWhiteSpace(Namespace);
    public bool ShouldSerializeModelType() => ModelType != ModelType.Primitive; // suppress for primitives
}

public class EnumMember
{
    [XmlAttribute("name")] public string Name { get; set; } = string.Empty;
    [XmlAttribute("value")] public string Value { get; set; } = string.Empty;
    [XmlAttribute("description")] public string? Description { get; set; }
    public bool ShouldSerializeDescription() => !string.IsNullOrWhiteSpace(Description);
}