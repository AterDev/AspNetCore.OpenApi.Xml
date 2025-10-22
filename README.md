# AspNetCore.OpenApi.Xml

为 ASP.NET Core 应用提供自定义 XML API 文档生成和 HTML 文档页面。

## 技术栈

- **框架**: ASP.NET Core (基于 .NET 9.0+)
- **语言**: C# 12+ with Nullable Reference Types
- **依赖**: Microsoft.AspNetCore.App Framework Reference
- **序列化**: System.Xml.Serialization
- **API 探索**: Microsoft.AspNetCore.Mvc.ApiExplorer

## 系统要求

- .NET 9.0 或更高版本
- 支持的操作系统: Windows, Linux, macOS
- IDE (可选): Visual Studio 2022, VS Code, Rider

## 开发环境设置

### 1. 安装 .NET SDK

确保已安装 .NET 9.0 SDK 或更高版本：

```bash
dotnet --version
```

如需安装，请访问 [.NET 下载页面](https://dotnet.microsoft.com/download)。

### 2. 克隆仓库

```bash
git clone https://github.com/AterDev/AspNetCore.OpenApi.Xml.git
cd AspNetCore.OpenApi.Xml
```

### 3. 还原依赖

```bash
dotnet restore
```

### 4. 构建项目

```bash
dotnet build
```

### 5. 运行示例项目

```bash
cd Demo.WebApi
dotnet run
```

访问 `http://localhost:5276/api-doc` 查看生成的文档页面。

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

## 测试

目前项目主要通过 Demo.WebApi 项目进行功能验证。

### 运行测试场景

启动 Demo.WebApi 项目后，可以访问以下端点测试不同功能：

```bash
# 访问 HTML 文档页面
curl http://localhost:5276/api-doc

# 获取 XML 格式文档
curl http://localhost:5276/__api-doc.xml

# 测试泛型类型（详见 GENERIC_TESTS.md）
curl http://localhost:5276/api/GenericsTest/kvp
curl http://localhost:5276/api/GenericsTest/nested-dict
```

## 项目结构

```
AspNetCore.OpenApi.Xml/
├── OpenApi.Xml.Core/              # 核心领域模型和序列化
│   ├── Models/                    # API 文档数据模型
│   │   └── ApiDocument.cs        # 文档、端点、请求、响应、Schema 等
│   └── Serialization/            # XML 序列化/反序列化
│       └── ApiDocumentSerializer.cs
├── AspNetCore.OpenApi.Xml/        # ASP.NET Core 集成
│   ├── Services/                  # 核心服务
│   │   ├── ApiXmlDocumentGenerator.cs      # 文档生成器
│   │   └── ApiDocumentationPageService.cs  # HTML 页面服务
│   └── Extensions/               # DI 和路由扩展
│       ├── ServiceCollectionExtensions.cs
│       └── EndpointRouteBuilderExtensions.cs
└── Demo.WebApi/                   # 演示项目
    └── Controllers/               # 示例控制器
```

## 贡献指南

欢迎贡献！请遵循以下准则：

### 代码规范

- 使用 C# 12+ 语法特性（collection expressions, primary constructors, expression-bodied members 等）
- 启用 Nullable Reference Types (`<Nullable>enable</Nullable>`)
- 遵循现有代码风格和命名约定
- 为公共 API 添加 XML 文档注释

### 提交 PR 流程

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

### 构建检查

在提交 PR 前，请确保：

```bash
# 构建成功
dotnet build

# 代码能正常运行
cd Demo.WebApi
dotnet run
```

## 路线图

- [ ] 添加单元测试和集成测试
- [ ] 支持更多 DataAnnotations 验证规则
- [ ] 循环引用检测和 $ref 引用支持
- [ ] 支持导出 Markdown 格式文档
- [ ] 性能优化（Source Generator 预构建 Schema）
- [ ] 支持自定义主题和样式

## 常见问题

### Q: 如何自定义文档页面的样式？

A: 目前 HTML 页面使用内联样式。未来版本将支持自定义主题。

### Q: 支持哪些验证规则？

A: 目前支持 `[Required]`、`[StringLength]`、`[Range]`、`[RegularExpression]` 等常用 DataAnnotations。

### Q: 如何处理复杂的泛型类型？

A: 系统支持泛型类型、嵌套泛型、字典等。详见 `Demo.WebApi/GENERIC_TESTS.md`。

## 许可证

MIT License - 详见 [LICENSE](LICENSE) 文件
