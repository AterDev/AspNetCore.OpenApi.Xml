# 项目概述 - AspNetCore.OpenApi.Xml

该仓库提供一个最小化的 **ASP.NET Core API Explorer → 自定义 XML 接口文档** 生成方案，绕过 OpenAPI/Swagger 规范，直接输出简洁 XML，并支持反序列化回对象模型，便于在内部平台、专有集成、离线分发等场景使用。

## 组成结构
```
AspNetCore.OpenApi.Xml.sln
├─ OpenApi.Xml.Core                // 文档领域模型与通用序列化
│  ├─ Models/ApiDocument.cs        // ApiDocument / Endpoint / ApiRequest / ApiResponse / ApiSchema / ApiField
│  └─ Serialization/ApiDocumentSerializer.cs
└─ AspNetCore.OpenApi.Xml          // 基于 ApiExplorer 的文档生成器及 DI 扩展
   ├─ Services/ApiXmlDocumentGenerator.cs
   └─ Extensions/ServiceCollectionExtensions.cs
```

## 核心模型 (Models)
- ApiDocument: 文档根（Title, Version, Endpoints）
- Endpoint: Path, Method, Deprecated, Summary, Description, Tags, Request, Responses
- ApiRequest: Headers, QueryParameters, RouteParameters, Body, Description
- ApiResponse: StatusCode, ContentType, Description, Body
- ApiField: Name, Type, Required, DefaultValue, Description, Example
- ApiSchema: Type(object/array/string/number/...), Nullable, Format, Description, Fields, ArrayItem

## 生成逻辑 (ApiXmlDocumentGenerator)
1. 遍历 IApiDescriptionGroupCollectionProvider.ApiDescriptionGroups
2. 读取 API 基础信息：Route(RelativePath)、HttpMethod、DisplayName、分组、过时标记(Obsolete)
3. 分类参数：Route(Query/Header/Route/Body)
4. 生成请求体 Schema（反射 + 递归，最大深度 8）
5. 组合响应信息 SupportedResponseTypes → ApiResponse 列表
6. 输出 ApiDocument -> XmlSerializer → XML 字符串

## Schema 构建策略
- 基本类型映射：string / integer / number / boolean / enum / object / array
- 集合检测：数组或实现 IEnumerable 且非字符串 → array
- 对象：公开实例属性转为 ApiField
- 限制最大递归深度 (8) 防止复杂循环结构无限展开

## 序列化 / 反序列化
提供静态工具类 ApiDocumentSerializer：
```csharp
var xml = ApiDocumentSerializer.ToXml(doc);
var doc2 = ApiDocumentSerializer.FromXml(xml);
```

## DI 注册与使用
```csharp
builder.Services.AddApiXmlDocumentGenerator();

app.MapGet("/__api-doc.xml", (IApiXmlDocumentGenerator gen) =>
    Results.Text(gen.GenerateXml("My API", "v1"), "application/xml"));
```

## 设计取舍
| 方面 | 决策 | 原因 |
|------|------|------|
| 不使用 OpenAPI | 自定义轻量 XML | 减少依赖，结构可控 |
| HttpMethod 用 string | 避免额外序列化处理 | 简化 XML 输出 |
| 多响应支持 | List<ApiResponse> | 兼容 200/400/500 等场景 |
| 简单 Required 判定 | 仅 `[Required]` | 后续可扩展 DataAnnotations/可空流分析 |
| 递归深度限制 | 8 | 避免循环或超复杂对象造成性能风险 |

## 潜在改进点
- 循环引用检测 (HashSet<Type>) + `$ref` 风格引用
- Enum 值列表输出
- DataAnnotations 约束（Range / StringLength / MaxLength）写入字段描述
- 更丰富的媒体类型与多 body 解析
- 过滤/分组/Tag 扩展（如：按自定义特性隐藏端点）
- Source Generator 预构建 Schema（提升启动性能）
- 输出格式扩展：Markdown / HTML(XSLT) / JSON 中间格式

## 贡献指南
1. Fork & 新建分支(feature/* 或 fix/*)
2. 代码需通过 `dotnet build`
3. 保持 `<Nullable>enable</Nullable>` 与当前风格（表达式体、集合表达式）
4. 新增功能附带文档更新

## 快速定位参考
| 任务 | 代码位置 |
|------|----------|
| 修改模型 | OpenApi.Xml.Core/Models/ApiDocument.cs |
| 增加序列化能力 | OpenApi.Xml.Core/Serialization/ApiDocumentSerializer.cs |
| 生成逻辑扩展 | AspNetCore.OpenApi.Xml/Services/ApiXmlDocumentGenerator.cs |
| 注入扩展 | AspNetCore.OpenApi.Xml/Extensions/ServiceCollectionExtensions.cs |

## License
待补充（如 MIT）。

---
该文档供 GitHub Copilot / 开发者快速理解工程意图与结构。