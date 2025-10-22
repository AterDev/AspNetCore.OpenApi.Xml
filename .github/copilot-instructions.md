# GitHub Copilot Instructions for AspNetCore.OpenApi.Xml

This document provides guidance for GitHub Copilot when working with the AspNetCore.OpenApi.Xml project.

## Project Overview

AspNetCore.OpenApi.Xml is a lightweight library that generates custom XML API documentation for ASP.NET Core applications, bypassing OpenAPI/Swagger specifications. It also provides an interactive HTML documentation page.

### Core Purpose
- Generate XML-based API documentation from ASP.NET Core ApiExplorer
- Provide a simple, self-contained alternative to Swagger/OpenAPI
- Support serialization/deserialization of API documentation
- Offer an interactive HTML documentation viewer

## Architecture

### Three-Layer Structure

1. **OpenApi.Xml.Core** (Domain Models)
   - Pure .NET library with no ASP.NET dependencies
   - Contains data models and serialization logic
   - Models: `ApiDocument`, `Endpoint`, `ApiRequest`, `ApiResponse`, `ApiSchema`, `ApiField`, `ApiModel`

2. **AspNetCore.OpenApi.Xml** (ASP.NET Integration)
   - Integrates with ASP.NET Core
   - Services: `ApiXmlDocumentGenerator`, `ApiDocumentationPageService`
   - Extensions: DI registration and endpoint mapping

3. **Demo.WebApi** (Example Application)
   - Demonstrates library usage
   - Contains test controllers for various scenarios

## Code Conventions

### Language Features
- **Target Framework**: .NET 9.0+ (C# 12+)
- **Nullable Reference Types**: Enabled (`<Nullable>enable</Nullable>`)
- **Preferred C# Features**:
  - Collection expressions: `[]` instead of `new List<T>()`
  - Primary constructors for dependency injection
  - Expression-bodied members for simple properties/methods
  - Pattern matching and switch expressions
  - File-scoped namespaces

### Coding Style
```csharp
// ✅ Good: Collection expression
public List<string> Tags { get; set; } = [];

// ❌ Avoid: Old-style initialization
public List<string> Tags { get; set; } = new List<string>();

// ✅ Good: Primary constructor
public class ApiXmlDocumentGenerator(IApiDescriptionGroupCollectionProvider provider) : IApiXmlDocumentGenerator
{
    // Direct use of constructor parameter
}

// ✅ Good: Expression-bodied property
public bool IsNullable => Type?.Contains("?") ?? false;

// ✅ Good: File-scoped namespace
namespace AspNetCore.OpenApi.Xml.Services;

public class MyService { }
```

### Naming Conventions
- **Classes/Interfaces**: PascalCase (e.g., `ApiXmlDocumentGenerator`, `IApiXmlDocumentGenerator`)
- **Methods/Properties**: PascalCase (e.g., `GenerateXml`, `OperationId`)
- **Parameters/Variables**: camelCase (e.g., `provider`, `modelCache`)
- **Private Fields**: _camelCase with underscore prefix (e.g., `_modelCache`, `_primitiveTypes`)
- **Constants**: PascalCase or UPPER_CASE for static readonly fields

### XML Documentation
- All public APIs must have XML documentation comments
- Use `<summary>`, `<param>`, and `<returns>` tags
- Example:
```csharp
/// <summary>
/// Generates an API document from discovered endpoints.
/// </summary>
/// <param name="title">The document title.</param>
/// <param name="version">The API version.</param>
/// <returns>The generated API document.</returns>
public ApiDocument Generate(string? title = null, string? version = null)
```

## Key Design Patterns

### 1. Schema Generation Strategy
- Recursive reflection with max depth limit (8 levels)
- Caching of processed types to avoid duplication
- Special handling for:
  - Primitive types (string, int, bool, DateTime, etc.)
  - Collections and arrays
  - Generic types (List<T>, Dictionary<K,V>, etc.)
  - Enums
  - Complex objects

### 2. Type Mapping
```csharp
// Primitive types map to simple type names
typeof(string) → "string"
typeof(int) → "integer"
typeof(bool) → "boolean"
typeof(DateTime) → "string" (format: "date-time")

// Arrays and collections
List<T> → type: "array", arrayItem: BuildSchema(T)
Dictionary<K,V> → type: "object", additionalProperties: BuildSchema(V)

// Complex types
Custom classes → type: "object", fields: properties
```

### 3. Validation Rules
Extract from DataAnnotations attributes:
- `[Required]` → Required: true
- `[StringLength]` → MinLength, MaxLength
- `[Range]` → Minimum, Maximum
- `[RegularExpression]` → Pattern

## Common Tasks

### Adding New Model Properties
When adding properties to models in `OpenApi.Xml.Core/Models/`:
1. Add XML serialization attributes (`[XmlAttribute]` or `[XmlElement]`)
2. Initialize with default values (use `= []` for collections)
3. Add XML documentation comments
4. Consider nullable reference types

### Extending Schema Generation
When modifying `ApiXmlDocumentGenerator.BuildSchema()`:
1. Check if type is already in `_modelCache` to prevent duplicates
2. Respect max depth limit to avoid infinite recursion
3. Handle nullable types properly
4. Add validation rule extraction if needed

### Adding Service Extensions
Follow the pattern in `ServiceCollectionExtensions.cs`:
```csharp
public static IServiceCollection AddMyFeature(this IServiceCollection services)
{
    services.AddSingleton<IMyService, MyService>();
    return services;
}
```

### Adding Endpoint Extensions
Follow the pattern in `EndpointRouteBuilderExtensions.cs`:
```csharp
public static IEndpointRouteBuilder MapMyEndpoint(
    this IEndpointRouteBuilder endpoints,
    string pattern = "/my-endpoint")
{
    endpoints.MapGet(pattern, async context => { /* ... */ });
    return endpoints;
}
```

## Testing Guidance

### Manual Testing
Use the Demo.WebApi project for testing:
1. Start the application: `cd Demo.WebApi && dotnet run`
2. Access HTML docs: `http://localhost:5276/api-doc`
3. Access XML endpoint: `http://localhost:5276/__api-doc.xml`

### Test Scenarios
- Generic types: See `Demo.WebApi/GENERIC_TESTS.md`
- Complex nested objects
- DataAnnotations validation rules
- Different HTTP methods (GET, POST, PUT, DELETE)
- Route parameters, query parameters, request bodies

## Performance Considerations

### Current Optimizations
- Type caching in `_modelCache` to avoid regenerating schemas
- Static `_primitiveTypes` HashSet for O(1) lookups
- Lazy schema generation (only when needed)

### Known Limitations
- Max recursion depth: 8 levels
- No circular reference detection (relies on depth limit)
- Schema built at runtime (not pre-generated)

### Future Improvements
- Implement circular reference detection with HashSet<Type>
- Consider Source Generators for compile-time schema generation
- Add caching for entire ApiDocument (not just models)

## Common Pitfalls to Avoid

### ❌ Don't
- Remove or modify XML serialization attributes without testing
- Increase max recursion depth beyond 8 without circular reference detection
- Use blocking I/O operations in async contexts
- Ignore nullable reference type warnings
- Hard-code magic strings (use constants or config)

### ✅ Do
- Test changes with Demo.WebApi project
- Check both HTML and XML output after changes
- Use readonly collections where appropriate
- Handle edge cases (null values, empty collections, etc.)
- Follow existing patterns for consistency

## File Organization

### Where to Add New Code

| Feature Type | Location |
|--------------|----------|
| New data model | `OpenApi.Xml.Core/Models/` |
| Serialization logic | `OpenApi.Xml.Core/Serialization/` |
| Document generation | `AspNetCore.OpenApi.Xml/Services/ApiXmlDocumentGenerator.cs` |
| HTML page rendering | `AspNetCore.OpenApi.Xml/Services/ApiDocumentationPageService.cs` |
| DI extensions | `AspNetCore.OpenApi.Xml/Extensions/ServiceCollectionExtensions.cs` |
| Endpoint mapping | `AspNetCore.OpenApi.Xml/Extensions/EndpointRouteBuilderExtensions.cs` |
| Example controllers | `Demo.WebApi/Controllers/` |

## Dependencies

### Core Libraries
- `Microsoft.AspNetCore.App` (framework reference)
- `System.Xml.Serialization` (included in .NET)

### No External NuGet Packages
The project intentionally avoids external dependencies to remain lightweight and self-contained.

## Questions & Context

When encountering unclear requirements:
1. Check `PROJECT_SUMMARY.md` for architectural decisions
2. Review `Demo.WebApi/GENERIC_TESTS.md` for test scenarios
3. Examine existing code patterns in similar features
4. Consider backward compatibility with existing XML output format

## Quick Reference

### Build and Run
```bash
# Build entire solution
dotnet build

# Run demo application
cd Demo.WebApi && dotnet run

# Restore packages
dotnet restore
```

### Key Namespaces
- `OpenApi.Xml.Core.Models` - Data models
- `OpenApi.Xml.Core.Serialization` - XML serialization
- `AspNetCore.OpenApi.Xml.Services` - Core services
- `AspNetCore.OpenApi.Xml.Extensions` - DI and routing extensions

### Important Interfaces
- `IApiXmlDocumentGenerator` - Main document generation interface
- `IApiDescriptionGroupCollectionProvider` - ASP.NET Core API explorer

This document should be updated as the project evolves. When making significant architectural changes, please update this file accordingly.
