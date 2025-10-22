# Development Workflows - AspNetCore.OpenApi.Xml

This guide provides step-by-step instructions for common development tasks in the AspNetCore.OpenApi.Xml project.

## Setup and Environment

### Initial Setup
```bash
# Clone the repository
git clone https://github.com/AterDev/AspNetCore.OpenApi.Xml.git
cd AspNetCore.OpenApi.Xml

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the demo
cd Demo.WebApi
dotnet run
```

### Verify Installation
1. Build succeeds without errors: `dotnet build`
2. Demo runs successfully: `cd Demo.WebApi && dotnet run`
3. Access http://localhost:5276/api-doc - HTML page loads
4. Access http://localhost:5276/__api-doc.xml - XML is generated

## Common Development Workflows

### Workflow 1: Adding Support for a New DataAnnotation

**Scenario**: You want to extract a new validation attribute (e.g., `[EmailAddress]`)

**Steps**:

1. **Locate the validation extraction code**
   ```bash
   # Open the document generator
   code AspNetCore.OpenApi.Xml/Services/ApiXmlDocumentGenerator.cs
   ```

2. **Find the `ExtractValidationRules` or property building method**
   - Look for where `[Required]`, `[StringLength]`, etc. are handled
   - Usually in the `BuildField` or `BuildSchema` methods

3. **Add the new attribute check**
   ```csharp
   // Example: Adding EmailAddress support
   var emailAttr = property.GetCustomAttribute<EmailAddressAttribute>();
   if (emailAttr != null)
   {
       field.Format = "email";
       field.Pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // Simple email regex
   }
   ```

4. **Add the property to ApiField if needed**
   - If the attribute needs a new field (like `Format`), add it to the model:
   ```csharp
   // In OpenApi.Xml.Core/Models/ApiDocument.cs
   [XmlAttribute("format")]
   public string? Format { get; set; }
   ```

5. **Test the change**
   ```bash
   # Build
   dotnet build
   
   # Add a test DTO in Demo.WebApi
   cat > Demo.WebApi/Models/TestDto.cs << 'EOF'
   public class TestDto
   {
       [EmailAddress]
       public string Email { get; set; } = string.Empty;
   }
   EOF
   
   # Add test controller endpoint
   # Run and verify output
   cd Demo.WebApi && dotnet run
   ```

### Workflow 2: Adding a New Complex Type Support

**Scenario**: You want to improve handling of a specific generic type (e.g., `Task<T>`)

**Steps**:

1. **Identify where generic types are handled**
   ```bash
   # Find the BuildSchema method
   grep -n "BuildSchema" AspNetCore.OpenApi.Xml/Services/ApiXmlDocumentGenerator.cs
   ```

2. **Add special handling before general generic handling**
   ```csharp
   private ApiSchema BuildSchema(Type type, int depth = 0, int maxDepth = 8)
   {
       // ... existing checks ...
       
       // Handle Task<T> - unwrap to inner type
       if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
       {
           var innerType = type.GetGenericArguments()[0];
           return BuildSchema(innerType, depth, maxDepth);
       }
       
       // ... continue with existing logic ...
   }
   ```

3. **Add a test case**
   ```csharp
   // In Demo.WebApi/Controllers/
   [HttpGet("async-test")]
   public async Task<UserDto> GetAsync()
   {
       await Task.Delay(10);
       return new UserDto();
   }
   ```

4. **Verify the output**
   - Run Demo.WebApi
   - Check XML output shows `UserDto` schema, not `Task`
   - Check HTML page displays correctly

### Workflow 3: Customizing HTML Documentation Page

**Scenario**: You want to add a new section or change styling

**Steps**:

1. **Open the HTML service**
   ```bash
   code AspNetCore.OpenApi.Xml/Services/ApiDocumentationPageService.cs
   ```

2. **Locate the HTML template section**
   - The HTML is embedded as a string
   - CSS is inline in `<style>` tags
   - JavaScript is inline in `<script>` tags

3. **Make your changes**
   ```csharp
   // Example: Adding a custom header
   var html = $@"
   <!DOCTYPE html>
   <html>
   <head>
       <!-- existing head content -->
   </head>
   <body>
       <div class=""custom-header"">
           <h2>Custom Header Text</h2>
       </div>
       <!-- existing body content -->
   ";
   ```

4. **Update styles**
   ```css
   /* Add in the <style> section */
   .custom-header {
       background: #f6f8fa;
       padding: 1rem;
       border-bottom: 1px solid #e1e4e8;
   }
   ```

5. **Test in browser**
   ```bash
   cd Demo.WebApi
   dotnet run
   # Open http://localhost:5276/api-doc
   # Verify changes appear correctly
   # Test responsive design (resize browser)
   ```

### Workflow 4: Adding a New Extension Method

**Scenario**: You want to add a new way to configure or use the library

**Steps**:

1. **Determine the extension type**
   - Service registration? → `ServiceCollectionExtensions.cs`
   - Endpoint mapping? → `EndpointRouteBuilderExtensions.cs`

2. **Add the method**
   ```csharp
   // In AspNetCore.OpenApi.Xml/Extensions/ServiceCollectionExtensions.cs
   
   /// <summary>
   /// Adds API XML document generator with custom options.
   /// </summary>
   public static IServiceCollection AddApiXmlDocumentGenerator(
       this IServiceCollection services,
       Action<XmlDocumentOptions> configure)
   {
       var options = new XmlDocumentOptions();
       configure(options);
       
       services.AddSingleton(options);
       services.AddSingleton<IApiXmlDocumentGenerator, ApiXmlDocumentGenerator>();
       
       return services;
   }
   ```

3. **Create options class if needed**
   ```csharp
   // Create new file: AspNetCore.OpenApi.Xml/Options/XmlDocumentOptions.cs
   namespace AspNetCore.OpenApi.Xml.Options;
   
   public class XmlDocumentOptions
   {
       public int MaxDepth { get; set; } = 8;
       public bool IncludeObsolete { get; set; } = true;
   }
   ```

4. **Update the service to use options**
   ```csharp
   // Modify ApiXmlDocumentGenerator constructor
   public class ApiXmlDocumentGenerator(
       IApiDescriptionGroupCollectionProvider provider,
       XmlDocumentOptions? options = null) : IApiXmlDocumentGenerator
   {
       private readonly int _maxDepth = options?.MaxDepth ?? 8;
       // ...
   }
   ```

5. **Test the new API**
   ```csharp
   // In Program.cs
   builder.Services.AddApiXmlDocumentGenerator(options =>
   {
       options.MaxDepth = 10;
       options.IncludeObsolete = false;
   });
   ```

### Workflow 5: Debugging Document Generation Issues

**Scenario**: Generated XML or HTML is incorrect or missing data

**Steps**:

1. **Enable detailed logging**
   ```csharp
   // Add logging to ApiXmlDocumentGenerator
   using Microsoft.Extensions.Logging;
   
   public class ApiXmlDocumentGenerator(
       IApiDescriptionGroupCollectionProvider provider,
       ILogger<ApiXmlDocumentGenerator> logger) : IApiXmlDocumentGenerator
   {
       public ApiDocument Generate(string? title = null, string? version = null)
       {
           logger.LogInformation("Starting document generation");
           // ... existing code ...
           logger.LogInformation("Generated {Count} endpoints", doc.Endpoints.Count);
       }
   }
   ```

2. **Add debug output**
   ```csharp
   // Temporary debug code
   foreach (var group in provider.ApiDescriptionGroups.Items)
   {
       logger.LogDebug("Group: {GroupName}", group.GroupName);
       foreach (var api in group.Items)
       {
           logger.LogDebug("  API: {Method} {Path}", 
               api.HttpMethod, api.RelativePath);
       }
   }
   ```

3. **Inspect the ApiDescription data**
   ```csharp
   // In Demo.WebApi/Program.cs, add temporary endpoint
   app.MapGet("/debug-api-explorer", (IApiDescriptionGroupCollectionProvider provider) =>
   {
       var data = provider.ApiDescriptionGroups.Items
           .SelectMany(g => g.Items)
           .Select(api => new
           {
               api.HttpMethod,
               api.RelativePath,
               api.ActionDescriptor?.DisplayName,
               Parameters = api.ParameterDescriptions.Select(p => new
               {
                   p.Name,
                   p.Type?.Name,
                   p.Source
               })
           });
       return Results.Json(data);
   });
   ```

4. **Check XML serialization**
   ```bash
   # Get the raw XML
   curl http://localhost:5276/__api-doc.xml > output.xml
   
   # Validate XML structure
   xmllint --format output.xml
   
   # Check for serialization errors in the XML
   ```

5. **Use the object model directly**
   ```csharp
   // Test serialization/deserialization
   var doc = generator.Generate("Test", "v1");
   var xml = ApiDocumentSerializer.ToXml(doc);
   var deserialized = ApiDocumentSerializer.FromXml(xml);
   
   // Compare original and deserialized
   Console.WriteLine($"Original endpoints: {doc.Endpoints.Count}");
   Console.WriteLine($"Deserialized endpoints: {deserialized.Endpoints.Count}");
   ```

### Workflow 6: Adding Test Controllers and DTOs

**Scenario**: You need to test a new feature with example code

**Steps**:

1. **Create a test DTO**
   ```bash
   # Create new file
   cat > Demo.WebApi/Models/MyTestDto.cs << 'EOF'
   using System.ComponentModel.DataAnnotations;
   
   namespace Demo.WebApi.Models;
   
   public class MyTestDto
   {
       [Required]
       [StringLength(100, MinimumLength = 3)]
       public string Name { get; set; } = string.Empty;
       
       [Range(0, 150)]
       public int Age { get; set; }
       
       public List<string> Tags { get; set; } = [];
   }
   EOF
   ```

2. **Create a test controller**
   ```bash
   cat > Demo.WebApi/Controllers/MyTestController.cs << 'EOF'
   using Microsoft.AspNetCore.Mvc;
   using Demo.WebApi.Models;
   
   namespace Demo.WebApi.Controllers;
   
   [ApiController]
   [Route("api/[controller]")]
   public class MyTestController : ControllerBase
   {
       [HttpGet]
       public ActionResult<List<MyTestDto>> GetAll()
       {
           return Ok(new List<MyTestDto>());
       }
       
       [HttpPost]
       public ActionResult<MyTestDto> Create([FromBody] MyTestDto dto)
       {
           return Ok(dto);
       }
   }
   EOF
   ```

3. **Run and verify**
   ```bash
   cd Demo.WebApi
   dotnet run
   # Access http://localhost:5276/api-doc
   # Look for "MyTest" controller in the list
   # Click on endpoints to see details
   # Verify validation rules appear correctly
   ```

4. **Document the test**
   ```bash
   # Add to GENERIC_TESTS.md or create a new test documentation file
   echo "## MyTest Controller" >> Demo.WebApi/MY_TESTS.md
   echo "Tests validation rules display..." >> Demo.WebApi/MY_TESTS.md
   ```

## Build and Release Workflows

### Local Build
```bash
# Clean build
dotnet clean
dotnet build --configuration Release

# Check for warnings
dotnet build --configuration Release /p:TreatWarningsAsErrors=true
```

### Creating a NuGet Package (for maintainers)
```bash
# Pack the main library
cd AspNetCore.OpenApi.Xml
dotnet pack --configuration Release

# Pack the core library
cd ../OpenApi.Xml.Core
dotnet pack --configuration Release

# Packages are in bin/Release/*.nupkg
```

## Troubleshooting Common Issues

### Issue: Build fails with nullable reference warnings
**Solution**: 
- Enable nullable analysis: `<Nullable>enable</Nullable>` in .csproj
- Fix warnings by adding `?` to nullable types or initializing with defaults

### Issue: XML serialization fails
**Solution**:
- Ensure all properties have XML attributes (`[XmlAttribute]` or `[XmlElement]`)
- Check that types are XML-serializable (public parameterless constructor)
- Initialize collections with `= []`

### Issue: Infinite recursion / stack overflow
**Solution**:
- Verify `maxDepth` parameter is passed through recursive calls
- Check that depth is incremented: `BuildSchema(type, depth + 1, maxDepth)`
- Consider adding circular reference detection

### Issue: HTML page not displaying correctly
**Solution**:
- Check browser console for JavaScript errors
- Verify JSON data structure matches JavaScript expectations
- Test in different browsers
- Check for unclosed HTML tags

### Issue: Missing endpoints in documentation
**Solution**:
- Verify controller is discovered: check `IApiDescriptionGroupCollectionProvider`
- Ensure controller has `[ApiController]` attribute
- Check that endpoints have HTTP method attributes (`[HttpGet]`, etc.)
- Verify route templates are correct

## Code Quality Checks

### Before Committing
```bash
# Build without errors or warnings
dotnet build /p:TreatWarningsAsErrors=true

# Run demo to verify functionality
cd Demo.WebApi && dotnet run

# Manual checks:
# - [ ] HTML page loads correctly
# - [ ] XML endpoint returns valid XML
# - [ ] No console errors in browser
# - [ ] New features work as expected
```

### Code Style Checklist
- [ ] Using collection expressions `[]` instead of `new List<T>()`
- [ ] Using primary constructors for DI
- [ ] Using file-scoped namespaces
- [ ] XML documentation on public APIs
- [ ] Proper nullable reference type handling
- [ ] XML serialization attributes on model properties

## Getting Help

### Documentation Resources
1. `README.md` - User-facing documentation
2. `PROJECT_SUMMARY.md` - Architectural overview
3. `.github/copilot-instructions.md` - Copilot guidance
4. `.github/agents/coding-agent.md` - Detailed agent instructions
5. `Demo.WebApi/GENERIC_TESTS.md` - Test scenarios

### Code Navigation
```bash
# Find all usages of a type
grep -r "IApiXmlDocumentGenerator" --include="*.cs"

# Find all XML attributes
grep -r "\[Xml" --include="*.cs"

# Find all public APIs
grep -r "public.*class\|public.*interface" --include="*.cs"
```

### Common File Locations
| Task | File Path |
|------|-----------|
| Model definitions | `OpenApi.Xml.Core/Models/ApiDocument.cs` |
| Document generation | `AspNetCore.OpenApi.Xml/Services/ApiXmlDocumentGenerator.cs` |
| HTML page | `AspNetCore.OpenApi.Xml/Services/ApiDocumentationPageService.cs` |
| DI extensions | `AspNetCore.OpenApi.Xml/Extensions/ServiceCollectionExtensions.cs` |
| Endpoint extensions | `AspNetCore.OpenApi.Xml/Extensions/EndpointRouteBuilderExtensions.cs` |
| Example controllers | `Demo.WebApi/Controllers/` |

This workflow guide should cover most common development scenarios. Update this document as new workflows emerge or existing ones change.
