# Coding Agent Instructions - AspNetCore.OpenApi.Xml

## Agent Identity and Role

You are a coding agent specialized in working with the AspNetCore.OpenApi.Xml project. Your role is to make precise, minimal changes to the codebase while maintaining consistency with existing patterns and architectural decisions.

## Project Quick Start

### What This Project Does
AspNetCore.OpenApi.Xml generates XML-based API documentation for ASP.NET Core applications as a lightweight alternative to Swagger/OpenAPI. It:
- Reads API metadata from ASP.NET Core's ApiExplorer
- Generates custom XML documentation format
- Provides interactive HTML documentation viewer
- Supports serialization/deserialization of documentation

### Tech Stack Summary
- **Language**: C# 12 with .NET 9.0+
- **Framework**: ASP.NET Core
- **Features**: Nullable reference types, primary constructors, collection expressions
- **No external dependencies** (only uses framework references)

## Essential Context for AI Agents

### Project Structure (Quick Reference)
```
OpenApi.Xml.Core/
  Models/ApiDocument.cs       ← Data models for API documentation
  Serialization/              ← XML serialization utilities

AspNetCore.OpenApi.Xml/
  Services/
    ApiXmlDocumentGenerator.cs      ← Main document generation logic
    ApiDocumentationPageService.cs  ← HTML page generation
  Extensions/                        ← DI and routing extensions

Demo.WebApi/
  Controllers/                       ← Test/example controllers
```

### Core Workflow
1. **API Discovery**: `IApiDescriptionGroupCollectionProvider` discovers endpoints
2. **Schema Building**: Reflection-based recursive schema generation (max depth: 8)
3. **Model Caching**: Types cached in `_modelCache` to avoid duplication
4. **Serialization**: `XmlSerializer` outputs XML format
5. **HTML Rendering**: Embedded HTML template with inline CSS/JavaScript

## Coding Patterns You Must Follow

### 1. Modern C# Syntax (Required)
```csharp
// ✅ ALWAYS use collection expressions
public List<string> Items { get; set; } = [];

// ✅ ALWAYS use primary constructors for DI
public class MyService(IDependency dep) : IMyService
{
    public void Method() => dep.DoSomething();
}

// ✅ ALWAYS use file-scoped namespaces
namespace AspNetCore.OpenApi.Xml.Services;

// ✅ Use expression bodies for simple members
public bool IsValid => !string.IsNullOrEmpty(Name);
```

### 2. Nullable Reference Types (Critical)
```csharp
// ✅ CORRECT: Nullable when value may be null
public string? OptionalValue { get; set; }

// ✅ CORRECT: Non-nullable with initialization
public string RequiredValue { get; set; } = string.Empty;

// ✅ CORRECT: Required in constructor
public required string MustBeSet { get; set; }
```

### 3. XML Serialization Attributes (Essential)
```csharp
// ✅ For XML attributes
[XmlAttribute("name")]
public string Name { get; set; } = string.Empty;

// ✅ For XML elements
[XmlElement("Field")]
public List<ApiField> Fields { get; set; } = [];

// ✅ For XML arrays
[XmlArray("Models")]
[XmlArrayItem("Model")]
public List<ApiModel> Models { get; set; } = [];
```

## Common Tasks and Patterns

### Task 1: Adding a New Property to a Model

**Location**: `OpenApi.Xml.Core/Models/ApiDocument.cs` (or related model)

**Pattern**:
```csharp
/// <summary>
/// Brief description of the property.
/// </summary>
[XmlAttribute("propertyName")] // or [XmlElement("PropertyName")]
public string PropertyName { get; set; } = string.Empty; // or []
```

**Checklist**:
- [ ] Add XML documentation comment
- [ ] Add XML serialization attribute
- [ ] Initialize with default value
- [ ] Consider nullable types

### Task 2: Extending Schema Generation

**Location**: `AspNetCore.OpenApi.Xml/Services/ApiXmlDocumentGenerator.cs`

**Key Points**:
- Check `_modelCache` before processing a type
- Respect `maxDepth` parameter to prevent stack overflow
- Handle nullable types: `Nullable.GetUnderlyingType(type)`
- Extract validation rules from DataAnnotations

**Pattern**:
```csharp
private ApiSchema BuildSchema(Type type, int depth = 0, int maxDepth = 8)
{
    // 1. Check depth limit
    if (depth > maxDepth) return new ApiSchema { Type = "object" };
    
    // 2. Check cache
    if (_modelCache.TryGetValue(type, out var cached))
        return new ApiSchema { Type = "object", Reference = cached.Name };
    
    // 3. Handle nullables
    var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
    
    // 4. Process type
    // ...
}
```

### Task 3: Adding a Service Extension

**Location**: `AspNetCore.OpenApi.Xml/Extensions/ServiceCollectionExtensions.cs`

**Pattern**:
```csharp
/// <summary>
/// Adds my feature to the service collection.
/// </summary>
/// <param name="services">The service collection.</param>
/// <returns>The service collection for chaining.</returns>
public static IServiceCollection AddMyFeature(this IServiceCollection services)
{
    services.AddSingleton<IMyService, MyService>();
    // Add other dependencies...
    return services;
}
```

### Task 4: Adding an Endpoint Extension

**Location**: `AspNetCore.OpenApi.Xml/Extensions/EndpointRouteBuilderExtensions.cs`

**Pattern**:
```csharp
/// <summary>
/// Maps my custom endpoint.
/// </summary>
public static IEndpointRouteBuilder MapMyEndpoint(
    this IEndpointRouteBuilder endpoints,
    string pattern = "/my-endpoint",
    string title = "My Title")
{
    endpoints.MapGet(pattern, async context =>
    {
        // Implementation
    });
    return endpoints;
}
```

## Type Handling Reference

### Primitive Type Detection
```csharp
private static readonly HashSet<Type> _primitiveTypes = new(
    new[]
    {
        typeof(string), typeof(bool), typeof(byte), typeof(sbyte),
        typeof(short), typeof(ushort), typeof(int), typeof(uint),
        typeof(long), typeof(ulong), typeof(float), typeof(double),
        typeof(decimal), typeof(Guid), typeof(DateTime),
        typeof(DateOnly), typeof(TimeOnly), typeof(TimeSpan)
    });
```

### Generic Type Mapping
- `List<T>` → array with item type T
- `Dictionary<K,V>` → object with additional properties of type V
- `KeyValuePair<K,V>` → object with Key and Value fields
- `Tuple<T1,T2,...>` → object with Item1, Item2, etc.
- `(T1, T2)` (ValueTuple) → object with named or Item1, Item2 fields

### Collection Detection
```csharp
// Check if type is a collection (but not string)
var isCollection = type != typeof(string) &&
                   typeof(IEnumerable).IsAssignableFrom(type);
```

## DataAnnotations Extraction

Extract validation rules from these attributes:
```csharp
// Required
[Required] → apiField.Required = true

// String length
[StringLength(100, MinimumLength = 5)]
→ apiField.MinLength = 5, MaxLength = 100

// Range
[Range(1, 100)]
→ apiField.Minimum = "1", Maximum = "100"

// Regular expression
[RegularExpression(@"^\d{3}$")]
→ apiField.Pattern = @"^\d{3}$"
```

## Testing Your Changes

### 1. Build Test
```bash
dotnet build
# Must succeed with no errors
```

### 2. Manual Testing
```bash
cd Demo.WebApi
dotnet run
# Visit: http://localhost:5276/api-doc
# Verify: HTML page renders correctly
# Check: XML endpoint http://localhost:5276/__api-doc.xml
```

### 3. Specific Feature Testing
- Add a test endpoint in `Demo.WebApi/Controllers/`
- Add test DTOs with various attributes
- Verify output in both HTML and XML

## Performance Constraints

### Max Recursion Depth: 8
**Why**: Prevents stack overflow on complex/circular types
**Impact**: Very deep nested objects will be truncated
**Don't**: Increase this without adding circular reference detection

### Type Caching
**Purpose**: Avoid regenerating schemas for same type
**Location**: `_modelCache` in `ApiXmlDocumentGenerator`
**Pattern**: Check cache before building, add after building

## Error Handling Patterns

### Don't Throw on Missing Data
```csharp
// ✅ GOOD: Graceful degradation
var description = apiDesc.ActionDescriptor?.DisplayName ?? "Unknown";

// ❌ BAD: Throwing exception
var description = apiDesc.ActionDescriptor.DisplayName!; // May be null
```

### Handle Reflection Safely
```csharp
// ✅ GOOD: Check for null
var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
foreach (var prop in properties ?? [])
{
    // ...
}
```

## HTML Generation Guidelines

**Location**: `AspNetCore.OpenApi.Xml/Services/ApiDocumentationPageService.cs`

**Current Approach**: Embedded HTML with inline CSS/JavaScript
**Why**: No external file dependencies, single-file deployment

**When Modifying**:
- Test in multiple browsers (Chrome, Firefox, Edge)
- Ensure responsive design works (mobile, tablet, desktop)
- Use semantic HTML
- Keep JavaScript minimal and compatible

## Common Mistakes to Avoid

### ❌ Mistake 1: Breaking XML Serialization
```csharp
// BAD: Missing XML attribute
public string NewProperty { get; set; }

// GOOD: With XML attribute
[XmlAttribute("newProperty")]
public string NewProperty { get; set; } = string.Empty;
```

### ❌ Mistake 2: Ignoring Nullability
```csharp
// BAD: Non-nullable but can be null
public string Name { get; set; }

// GOOD: Properly nullable
public string? Name { get; set; }
// OR with default
public string Name { get; set; } = string.Empty;
```

### ❌ Mistake 3: Not Initializing Collections
```csharp
// BAD: Null reference exception risk
public List<string> Items { get; set; }

// GOOD: Initialized
public List<string> Items { get; set; } = [];
```

### ❌ Mistake 4: Infinite Recursion
```csharp
// BAD: No depth check
private ApiSchema BuildSchema(Type type)
{
    var fields = GetProperties(type);
    foreach (var field in fields)
        field.Schema = BuildSchema(field.PropertyType); // Infinite loop!
}

// GOOD: Depth limit
private ApiSchema BuildSchema(Type type, int depth = 0, int maxDepth = 8)
{
    if (depth > maxDepth) return new ApiSchema { Type = "object" };
    // ...
}
```

## Quick Decision Tree

### When adding a feature, ask:

1. **Is it a model change?**
   - YES → Edit `OpenApi.Xml.Core/Models/`
   - Add XML attributes, XML comments, defaults

2. **Is it document generation logic?**
   - YES → Edit `ApiXmlDocumentGenerator.cs`
   - Follow recursive pattern, check cache, respect depth

3. **Is it HTML rendering?**
   - YES → Edit `ApiDocumentationPageService.cs`
   - Test in browser, keep inline styles

4. **Is it a public API?**
   - YES → Add extension method
   - Return `this` for chaining, add XML docs

5. **Does it need testing?**
   - Add example to `Demo.WebApi/Controllers/`
   - Update `GENERIC_TESTS.md` if applicable

## Files You'll Most Often Edit

1. **Models**: `OpenApi.Xml.Core/Models/ApiDocument.cs`
2. **Generation**: `AspNetCore.OpenApi.Xml/Services/ApiXmlDocumentGenerator.cs`
3. **HTML Page**: `AspNetCore.OpenApi.Xml/Services/ApiDocumentationPageService.cs`
4. **Extensions**: `AspNetCore.OpenApi.Xml/Extensions/*.cs`
5. **Examples**: `Demo.WebApi/Controllers/*.cs`

## Integration Points

### ASP.NET Core Integration
- `IApiDescriptionGroupCollectionProvider` - discovers endpoints
- `ApiDescription` - contains endpoint metadata
- `ParameterDescriptor` - contains parameter info
- `ControllerActionDescriptor` - contains action metadata

### Serialization Integration
- `XmlSerializer` - serializes/deserializes ApiDocument
- `XmlAttribute` - marks XML attributes
- `XmlElement` - marks XML elements
- `XmlArray` / `XmlArrayItem` - marks arrays

## Memory and Caching

**What's Cached**: Type schemas in `_modelCache`
**When Cleared**: Each call to `Generate()`
**Why**: Avoid duplicate model definitions in output

**Don't Cache**: ApiDocument instances (they're regenerated per request)

## Backward Compatibility

**XML Format**: Changing existing XML structure may break consumers
**Action**: Add new elements/attributes, don't remove existing ones
**Exception**: Bug fixes to obviously incorrect output

## Summary Checklist for Any Change

- [ ] Follows modern C# patterns (collection expressions, primary constructors)
- [ ] Nullable reference types handled correctly
- [ ] XML serialization attributes present on model changes
- [ ] XML documentation comments added for public APIs
- [ ] Respects max depth limit (8) for recursion
- [ ] Checks cache before generating duplicate schemas
- [ ] Tested with `dotnet build` - builds successfully
- [ ] Tested with Demo.WebApi - works correctly
- [ ] No external dependencies added
- [ ] Consistent with existing code style

## When in Doubt

1. Check `PROJECT_SUMMARY.md` for architectural context
2. Look at similar existing code for patterns
3. Test changes with Demo.WebApi
4. Verify XML output format is valid
5. Ensure HTML page still renders properly

This document provides everything you need to make effective, consistent changes to the AspNetCore.OpenApi.Xml codebase without lengthy analysis on each task.
