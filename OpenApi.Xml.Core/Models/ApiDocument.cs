using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenApi.Xml.Core.Models;

// API 文档的根节点
[XmlRoot("ApiDocument")]
public class ApiDocument
{
    [XmlAttribute("title")] public string Title { get; set; } = "API Documentation";
    [XmlAttribute("version")] public string Version { get; set; } = "1.0";

    [XmlElement("Endpoint")] public List<Endpoint> Endpoints { get; set; } = new();
}

// 单个接口
public class Endpoint
{
    [XmlAttribute("path")] public required string Path { get; set; }
    [XmlAttribute("method")] public required string Method { get; set; }

    [XmlAttribute("deprecated")] public bool Deprecated { get; set; }

    [XmlElement("Summary")] public string? Summary { get; set; }
    [XmlElement("Description")] public string? Description { get; set; }

    [XmlArray("Tags")]
    [XmlArrayItem("Tag")]
    public List<string>? Tags { get; set; }

    [XmlElement("Request")] public ApiRequest? Request { get; set; }

    [XmlArray("Responses")]
    [XmlArrayItem("Response")]
    public List<ApiResponse> Responses { get; set; } = new();
}

// 请求
public class ApiRequest
{
    [XmlElement("Header")] public List<ApiField> Headers { get; set; } = new();
    [XmlElement("Query")] public List<ApiField> QueryParameters { get; set; } = new();
    [XmlElement("Route")] public List<ApiField> RouteParameters { get; set; } = new();
    [XmlElement("Body")] public ApiSchema? Body { get; set; }
    [XmlElement("Description")] public string? Description { get; set; }
}

// 响应
public class ApiResponse
{
    [XmlAttribute("status")] public int StatusCode { get; set; }
    [XmlAttribute("contentType")] public string? ContentType { get; set; }
    [XmlElement("Description")] public string? Description { get; set; }
    [XmlElement("Body")] public ApiSchema? Body { get; set; }
}

// 通用的字段定义
public class ApiField
{
    [XmlAttribute("name")] public required string Name { get; set; }
    [XmlAttribute("type")] public string? Type { get; set; } // string/int/datetime/boolean/...


    [XmlAttribute("required")] public bool Required { get; set; }
    [XmlAttribute("default")] public string? DefaultValue { get; set; }
    [XmlElement("Description")] public string? Description { get; set; }
    [XmlElement("Example")] public string? Example { get; set; }
}

// 数据模型（Schema）
public class ApiSchema
{
    [XmlAttribute("type")] public required string Type { get; set; } // object/array/string/int/...

    [XmlAttribute("nullable")] public bool Nullable { get; set; }

    [XmlElement("Format")] public string? Format { get; set; }
    [XmlElement("Description")] public string? Description { get; set; }

    [XmlElement("Field")] public List<ApiField> Fields { get; set; } = new();
    [XmlElement("Item")] public ApiSchema? ArrayItem { get; set; } // 数组元素类型
}