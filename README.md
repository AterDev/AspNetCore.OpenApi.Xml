# AspNetCore.OpenApi.Xml

为 ASP.NET Core 应用提供自定义 XML API 文档生成和 HTML 文档页面。

## 系统要求

- .NET 10.0 或更高版本（支持预览版本）
- 如果使用 .NET 9.0，请将项目文件中的 `<TargetFramework>net10.0</TargetFramework>` 改为 `<TargetFramework>net9.0</TargetFramework>`

## 特性

- ✅ 基于 ASP.NET Core ApiExplorer 自动生成 API 文档
- ✅ 输出简洁的 XML 格式文档
- ✅ 提供交互式 HTML 文档页面
- ✅ 支持类型详情查看（模态框）
- ✅ 显示验证规则（长度、范围、正则表达式等）
- ✅ 按控制器分组的接口列表
- ✅ 响应式设计，GitHub 风格 UI

## 快速开始

### 1. 安装

```bash
dotnet add package AspNetCore.OpenApi.Xml
```

### 2. 配置

在 `Program.cs` 中注册服务并映射文档页面：

```csharp
using AspNetCore.OpenApi.Xml.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// 注册 API 文档生成器
builder.Services.AddApiXmlDocumentGenerator();

var app = builder.Build();

// 映射 HTML 文档页面（默认路径: /api-doc）
app.MapApiDocumentationPage();

// 可选：映射 XML 文档端点
app.MapGet("/__api-doc.xml", (IApiXmlDocumentGenerator gen) =>
    Results.Text(gen.GenerateXml("My API", "v1"), "application/xml"));

app.MapControllers();
app.Run();
```

### 3. 访问文档

启动应用后，访问：
- HTML 文档页面: `http://localhost:5000/api-doc`
- XML 文档: `http://localhost:5000/__api-doc.xml`

## 文档页面功能

### 接口列表
- 按控制器分组显示所有接口
- 支持折叠/展开分组
- 显示 HTTP 方法（GET、POST、PUT、DELETE 等）
- 显示路由路径

### 接口详情
点击接口后显示：
- HTTP 方法和完整路由
- 路由参数
- 查询参数
- 请求头
- 请求体（含字段类型和验证规则）
- 响应格式（状态码、内容类型、响应体）

### 类型查看
- 点击类型名称打开模态框
- 查看类型的所有字段
- 显示验证规则（最小/最大长度、范围、正则表达式等）
- 支持嵌套类型查看
- 显示枚举成员及描述

## 自定义配置

### 自定义文档路径

```csharp
app.MapApiDocumentationPage("/docs");
```

### 自定义标题和版本

```csharp
app.MapApiDocumentationPage("/api-doc", "My Custom API", "v2.0");
```

### 仅生成 XML 文档

```csharp
public class MyController : ControllerBase
{
    private readonly IApiXmlDocumentGenerator _generator;

    public MyController(IApiXmlDocumentGenerator generator)
    {
        _generator = generator;
    }

    [HttpGet("/api/doc.xml")]
    public IActionResult GetXmlDoc()
    {
        var xml = _generator.GenerateXml("My API", "v1");
        return Content(xml, "application/xml");
    }
}
```

### 程序化使用文档对象

```csharp
public class MyService
{
    private readonly IApiXmlDocumentGenerator _generator;

    public MyService(IApiXmlDocumentGenerator generator)
    {
        _generator = generator;
    }

    public void ProcessApiDoc()
    {
        var document = _generator.Generate("My API", "v1");
        
        // 访问所有端点
        foreach (var endpoint in document.Endpoints)
        {
            Console.WriteLine($"{endpoint.Method} {endpoint.Path}");
        }
        
        // 访问所有模型
        foreach (var model in document.Models)
        {
            Console.WriteLine($"Model: {model.Name}");
        }
    }
}
```

## 示例

查看 `Demo.WebApi` 项目获取完整示例。

运行示例：

```bash
cd Demo.WebApi
dotnet run
```

然后访问 `http://localhost:5276/api-doc`

## 许可证

[待补充]
