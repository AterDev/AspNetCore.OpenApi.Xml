# Quick Reference - AspNetCore.OpenApi.Xml

A quick lookup guide for common operations, patterns, and locations in the AspNetCore.OpenApi.Xml project.

## Project At-a-Glance

**What**: XML-based API documentation generator for ASP.NET Core
**How**: Reads from ApiExplorer, generates custom XML + HTML documentation
**Why**: Lightweight alternative to Swagger/OpenAPI with no external dependencies

## File Structure Map

```
AspNetCore.OpenApi.Xml/
│
├── OpenApi.Xml.Core/                    # Pure .NET models and serialization
│   ├── Models/
│   │   └── ApiDocument.cs              # ALL data models (ApiDocument, Endpoint, etc.)
│   └── Serialization/
│       └── ApiDocumentSerializer.cs    # XML serialize/deserialize
│
├── AspNetCore.OpenApi.Xml/              # ASP.NET Core integration
│   ├── Services/
│   │   ├── ApiXmlDocumentGenerator.cs        # 🔥 Main generation logic
│   │   └── ApiDocumentationPageService.cs    # HTML page generator
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs    # DI: AddApiXmlDocumentGenerator()
│       └── EndpointRouteBuilderExtensions.cs # Routing: MapApiDocumentationPage()
│
└── Demo.WebApi/                         # Example application
    ├── Controllers/                     # Test controllers
    ├── Program.cs                       # Usage example
    └── GENERIC_TESTS.md                 # Test documentation
```

## Key Classes and Interfaces

| Class/Interface | Purpose | Location |
|----------------|---------|----------|
| `IApiXmlDocumentGenerator` | Main service interface | `AspNetCore.OpenApi.Xml/Services/` |
| `ApiXmlDocumentGenerator` | Document generation implementation | `AspNetCore.OpenApi.Xml/Services/` |
| `ApiDocument` | Root model (Title, Version, Endpoints, Models) | `OpenApi.Xml.Core/Models/` |
| `Endpoint` | API endpoint metadata | `OpenApi.Xml.Core/Models/` |
| `ApiSchema` | Type schema (object/array/primitive) | `OpenApi.Xml.Core/Models/` |
| `ApiField` | Field/property metadata | `OpenApi.Xml.Core/Models/` |
| `ApiDocumentSerializer` | XML serialization utilities | `OpenApi.Xml.Core/Serialization/` |
| `ApiDocumentationPageService` | HTML page rendering | `AspNetCore.OpenApi.Xml/Services/` |

## Common Code Patterns

### Collection Initialization
```csharp
// ✅ Always use collection expressions
public List<string> Items { get; set; } = [];
public Dictionary<string, int> Map { get; set; } = [];
```

### Primary Constructors
```csharp
// ✅ Use for dependency injection
public class MyService(IDependency dep, ILogger<MyService> logger) : IMyService
{
    public void DoWork() => dep.Execute();
}
```

### Nullable Reference Types
```csharp
// ✅ Nullable when value can be null
public string? OptionalValue { get; set; }

// ✅ Non-null with default value
public string RequiredValue { get; set; } = string.Empty;

// ✅ Required property (must be set)
public required string MustBeSet { get; set; }
```

### XML Serialization
```csharp
// ✅ XML attribute
[XmlAttribute("name")]
public string Name { get; set; } = string.Empty;

// ✅ XML element
[XmlElement("Field")]
public ApiField Field { get; set; } = new();

// ✅ XML array
[XmlArray("Items")]
[XmlArrayItem("Item")]
public List<string> Items { get; set; } = [];
```

### Recursion with Depth Limit
```csharp
// ✅ Always include depth parameter
private ApiSchema BuildSchema(Type type, int depth = 0, int maxDepth = 8)
{
    if (depth > maxDepth) 
        return new ApiSchema { Type = "object" };
    
    // ... recursive calls with depth + 1
    return BuildSchema(childType, depth + 1, maxDepth);
}
```

## Type System Reference

### Primitive Types
```csharp
string, bool, byte, sbyte, short, ushort, int, uint,
long, ulong, float, double, decimal, 
Guid, DateTime, DateOnly, TimeOnly, TimeSpan
```

### Type Mappings
```csharp
// C# Type → Schema Type
string          → "string"
int, long       → "integer"
float, double   → "number"
bool            → "boolean"
DateTime        → "string" (format: "date-time")
Guid            → "string" (format: "uuid")
Enum            → "string" (enum values listed)
List<T>         → "array" (arrayItem: T schema)
Dictionary<K,V> → "object" (additionalProperties: V schema)
Custom class    → "object" (fields: properties)
```

### Generic Type Handling
```csharp
List<T>              → array of T
Dictionary<K,V>      → object with V-typed properties
KeyValuePair<K,V>    → object with Key and Value fields
Tuple<T1,T2>         → object with Item1, Item2 fields
ValueTuple (T1, T2)  → object with named or ItemN fields
Nullable<T>          → unwrap to T with nullable=true
Task<T>              → unwrap to T (async return handling)
```

## DataAnnotations Extraction

```csharp
[Required]                          → Required: true
[StringLength(100, MinimumLength=5)]→ MinLength: 5, MaxLength: 100
[Range(1, 100)]                     → Minimum: "1", Maximum: "100"
[RegularExpression(@"^\d+$")]       → Pattern: @"^\d+$"
[EmailAddress]                      → Format: "email"
[Url]                               → Format: "uri"
[Description("Text")]               → Description: "Text"
[DefaultValue("value")]             → DefaultValue: "value"
```

## API Usage Examples

### Basic Setup
```csharp
// In Program.cs
using AspNetCore.OpenApi.Xml.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddApiXmlDocumentGenerator();

var app = builder.Build();

// Map HTML documentation page
app.MapApiDocumentationPage(); // Default: /api-doc

// Or with custom path
app.MapApiDocumentationPage("/docs", "My API", "v2.0");

app.MapControllers();
app.Run();
```

### XML Endpoint
```csharp
// Expose XML directly
app.MapGet("/__api-doc.xml", (IApiXmlDocumentGenerator gen) =>
{
    var xml = gen.GenerateXml("My API", "v1");
    return Results.Text(xml, "application/xml");
});
```

### Programmatic Access
```csharp
// Inject and use the generator
public class MyService(IApiXmlDocumentGenerator generator)
{
    public void ProcessDocs()
    {
        // Get the document object
        var doc = generator.Generate("API", "v1");
        
        // Access endpoints
        foreach (var endpoint in doc.Endpoints)
        {
            Console.WriteLine($"{endpoint.Method} {endpoint.Path}");
        }
        
        // Serialize to XML
        var xml = ApiDocumentSerializer.ToXml(doc);
        
        // Deserialize from XML
        var restored = ApiDocumentSerializer.FromXml(xml);
    }
}
```

## Build Commands

```bash
# Restore packages
dotnet restore

# Build entire solution
dotnet build

# Build with warnings as errors
dotnet build /p:TreatWarningsAsErrors=true

# Build release version
dotnet build --configuration Release

# Run demo application
cd Demo.WebApi && dotnet run

# Clean build artifacts
dotnet clean
```

## Testing Endpoints

```bash
# HTML documentation page
curl http://localhost:5276/api-doc

# XML documentation
curl http://localhost:5276/__api-doc.xml

# With formatting (if xmllint installed)
curl http://localhost:5276/__api-doc.xml | xmllint --format -

# Save to file
curl http://localhost:5276/__api-doc.xml -o api-doc.xml
```

## Important Constants and Limits

```csharp
// Max recursion depth for schema building
const int MaxDepth = 8;

// Default endpoint paths
const string DefaultHtmlPath = "/api-doc";
const string DefaultXmlPath = "/__api-doc.xml";

// Default document values
const string DefaultTitle = "API Documentation";
const string DefaultVersion = "1.0";

// XML encoding
const string DefaultEncoding = "utf-8";
```

## Extension Method Signatures

```csharp
// Service registration
public static IServiceCollection AddApiXmlDocumentGenerator(
    this IServiceCollection services)

// HTML page mapping
public static IEndpointRouteBuilder MapApiDocumentationPage(
    this IEndpointRouteBuilder endpoints,
    string pattern = "/api-doc",
    string? title = null,
    string? version = null)
```

## Model Property Examples

```csharp
// ApiDocument
doc.Title       // "My API"
doc.Version     // "v1"
doc.Endpoints   // List<Endpoint>
doc.Models      // List<ApiModel>

// Endpoint
endpoint.OperationId  // "GetUser_1"
endpoint.Path         // "/api/users/{id}"
endpoint.Method       // "GET"
endpoint.Summary      // "Gets a user by ID"
endpoint.Tags         // ["Users"]
endpoint.Request      // ApiRequest
endpoint.Responses    // List<ApiResponse>

// ApiSchema
schema.Type           // "object" | "array" | "string" | "integer" | "number" | "boolean"
schema.Nullable       // true | false
schema.Fields         // List<ApiField>
schema.ArrayItem      // ApiSchema (for arrays)

// ApiField
field.Name            // "userId"
field.Type            // "integer"
field.Required        // true
field.MinLength       // 3
field.MaxLength       // 100
field.Pattern         // @"^\d+$"
```

## HTML Page Components

**Main Sections**:
1. Header with title and version
2. Endpoint list (grouped by controller)
3. Endpoint details panel
4. Type modal for schema inspection

**Interactive Features**:
- Click controller name to expand/collapse
- Click endpoint to show details
- Click type name to open modal
- Modal shows fields, validation rules, nested types

## Performance Tips

### Type Caching
```csharp
// Cache is cleared on each Generate() call
_modelCache.Clear();

// Types are cached during single generation
if (_modelCache.TryGetValue(type, out var cached))
    return new ApiSchema { Reference = cached.Name };
```

### Optimization Opportunities
- Schemas built lazily (only for used types)
- Primitive type check is O(1) with HashSet
- No circular reference detection yet (relies on depth limit)

## Debugging Quick Checks

```csharp
// 1. Check what ApiExplorer sees
app.MapGet("/debug", (IApiDescriptionGroupCollectionProvider provider) =>
{
    return provider.ApiDescriptionGroups.Items
        .SelectMany(g => g.Items)
        .Select(api => new { api.HttpMethod, api.RelativePath });
});

// 2. Check generated document
var doc = generator.Generate();
Console.WriteLine($"Endpoints: {doc.Endpoints.Count}");
Console.WriteLine($"Models: {doc.Models.Count}");

// 3. Validate XML
var xml = generator.GenerateXml();
var restored = ApiDocumentSerializer.FromXml(xml);
// Should not throw exception if XML is valid
```

## Common Gotchas

1. **Collections not initialized** → Null reference exception
   - Fix: Always initialize with `= []`

2. **Missing XML attributes** → Property not serialized
   - Fix: Add `[XmlAttribute]` or `[XmlElement]`

3. **Infinite recursion** → Stack overflow
   - Fix: Pass and check `depth` parameter

4. **Nullable warnings** → Build errors with nullable enabled
   - Fix: Add `?` for nullable or initialize with default

5. **Type not discovered** → Missing from documentation
   - Fix: Ensure type is used in public API surface

## Quick Navigation Commands

```bash
# Find all models
find . -path "*/Models/*.cs" -type f

# Find all services
find . -path "*/Services/*.cs" -type f

# Find all extensions
find . -path "*/Extensions/*.cs" -type f

# Find usage of specific type
grep -r "IApiXmlDocumentGenerator" --include="*.cs"

# Find all controllers
find . -path "*/Controllers/*.cs" -type f
```

## Version Information

- **Target Framework**: .NET 9.0
- **Language Version**: C# 12
- **Nullable Reference Types**: Enabled
- **Dependencies**: Microsoft.AspNetCore.App (framework reference only)

---

**Last Updated**: This reference reflects the current state of the project. Update when making significant changes.
