using OpenApi.Xml.Core.Models;

namespace AspNetCore.OpenApi.Xml.Helpers;

/// <summary>
/// Helper class for processing and formatting API model data for display purposes.
/// Handles type name generation, recursion depth checking, and display transformations.
/// </summary>
public static class ModelDisplayHelper
{
    /// <summary>
    /// Generates a simple type name string for display (without clickable links).
    /// Handles generic types, arrays, dictionaries, and tuples recursively.
    /// </summary>
    /// <param name="model">The API model to render</param>
    /// <param name="depth">Current recursion depth (default 0)</param>
    /// <param name="maxDepth">Maximum recursion depth (default 5)</param>
    /// <returns>Formatted type name string</returns>
    public static string GetTypeDisplayName(ApiModel? model, int depth = 0, int maxDepth = 5)
    {
        if (model == null) return "unknown";
        if (depth > maxDepth) return "...";

        var typeName = model.Name ?? model.Id ?? "unknown";

        // Handle Dictionary types
        if (model.KeyType != null && model.ValueType != null)
        {
            var keyTypeName = GetTypeDisplayName(model.KeyType, depth + 1, maxDepth);
            var valueTypeName = GetTypeDisplayName(model.ValueType, depth + 1, maxDepth);
            return $"{model.Name ?? "Dictionary"}<{keyTypeName}, {valueTypeName}>";
        }
        
        // Handle Array/List types
        if (model.ElementType != null)
        {
            var elemTypeName = GetTypeDisplayName(model.ElementType, depth + 1, maxDepth);
            return $"{model.Name ?? "Array"}<{elemTypeName}>";
        }
        
        // Handle Generic types
        if (model.GenericArguments != null && model.GenericArguments.Count > 0)
        {
            var genericParams = string.Join(", ", 
                model.GenericArguments.Select(arg => GetTypeDisplayName(arg, depth + 1, maxDepth)));
            return $"{model.Name ?? model.Id}<{genericParams}>";
        }

        return typeName;
    }

    /// <summary>
    /// Generates an HTML-encoded type name string suitable for display with angle brackets.
    /// </summary>
    /// <param name="model">The API model to render</param>
    /// <param name="depth">Current recursion depth (default 0)</param>
    /// <param name="maxDepth">Maximum recursion depth (default 5)</param>
    /// <returns>HTML-encoded type name string</returns>
    public static string GetTypeDisplayNameHtml(ApiModel? model, int depth = 0, int maxDepth = 5)
    {
        if (model == null) return "unknown";
        if (depth > maxDepth) return "...";

        var typeName = model.Name ?? model.Id ?? "unknown";

        // Handle Dictionary types
        if (model.KeyType != null && model.ValueType != null)
        {
            var keyTypeName = GetTypeDisplayName(model.KeyType, depth + 1, maxDepth);
            var valueTypeName = GetTypeDisplayName(model.ValueType, depth + 1, maxDepth);
            return $"{model.Name ?? "Dictionary"}&lt;{keyTypeName}, {valueTypeName}&gt;";
        }
        
        // Handle Array/List types
        if (model.ElementType != null)
        {
            var elemTypeName = GetTypeDisplayName(model.ElementType, depth + 1, maxDepth);
            return $"{model.Name ?? "Array"}&lt;{elemTypeName}&gt;";
        }
        
        // Handle Generic types
        if (model.GenericArguments != null && model.GenericArguments.Count > 0)
        {
            var genericParams = string.Join(", ", 
                model.GenericArguments.Select(arg => GetTypeDisplayName(arg, depth + 1, maxDepth)));
            return $"{model.Name ?? model.Id}&lt;{genericParams}&gt;";
        }

        return typeName;
    }

    /// <summary>
    /// Gets the model ID if the type is clickable (has fields or is a complex type).
    /// </summary>
    /// <param name="model">The API model to check</param>
    /// <returns>Model ID if clickable, null otherwise</returns>
    public static string? GetClickableModelId(ApiModel? model)
    {
        if (model == null) return null;
        
        // Only return ID for complex types that would have details to show
        if (string.IsNullOrEmpty(model.Id)) return null;
        
        // Don't make primitive types clickable
        if (model.ModelType == ModelType.Primitive) return null;
        
        return model.Id;
    }

    /// <summary>
    /// Determines if a model has displayable content (fields, enum members, etc.).
    /// </summary>
    /// <param name="model">The API model to check</param>
    /// <returns>True if the model has displayable content</returns>
    public static bool HasDisplayableContent(ApiModel? model)
    {
        if (model == null) return false;

        return (model.Fields != null && model.Fields.Count > 0) ||
               (model.EnumMembers != null && model.EnumMembers.Count > 0) ||
               (model.GenericArguments != null && model.GenericArguments.Count > 0) ||
               (model.TupleElements != null && model.TupleElements.Count > 0) ||
               model.ElementType != null ||
               (model.KeyType != null && model.ValueType != null);
    }

    /// <summary>
    /// Formats validation information for a field as a plain text string.
    /// </summary>
    /// <param name="field">The field to get validation info for</param>
    /// <returns>Formatted validation string, or empty string if no validation</returns>
    public static string GetValidationText(ApiField field)
    {
        var validations = new List<string>();
        
        if (field.MinLength.HasValue) 
            validations.Add($"最小长度: {field.MinLength}");
        
        if (field.MaxLength.HasValue) 
            validations.Add($"最大长度: {field.MaxLength}");
        
        if (!string.IsNullOrEmpty(field.Minimum)) 
            validations.Add($"最小值: {field.Minimum}");
        
        if (!string.IsNullOrEmpty(field.Maximum)) 
            validations.Add($"最大值: {field.Maximum}");
        
        if (!string.IsNullOrEmpty(field.Pattern)) 
            validations.Add($"模式: {field.Pattern}");
        
        return validations.Count > 0 ? string.Join(", ", validations) : string.Empty;
    }

    /// <summary>
    /// Gets localized validation text for a field.
    /// </summary>
    /// <param name="field">The field to get validation info for</param>
    /// <param name="language">Language code (e.g., "zh-CN", "en-US")</param>
    /// <returns>Formatted validation string</returns>
    public static string GetValidationTextLocalized(ApiField field, string language = "zh-CN")
    {
        var validations = new List<string>();
        
        var isChinese = language.StartsWith("zh");
        
        if (field.MinLength.HasValue)
            validations.Add(isChinese ? $"最小长度: {field.MinLength}" : $"Min Length: {field.MinLength}");
        
        if (field.MaxLength.HasValue)
            validations.Add(isChinese ? $"最大长度: {field.MaxLength}" : $"Max Length: {field.MaxLength}");
        
        if (!string.IsNullOrEmpty(field.Minimum))
            validations.Add(isChinese ? $"最小值: {field.Minimum}" : $"Minimum: {field.Minimum}");
        
        if (!string.IsNullOrEmpty(field.Maximum))
            validations.Add(isChinese ? $"最大值: {field.Maximum}" : $"Maximum: {field.Maximum}");
        
        if (!string.IsNullOrEmpty(field.Pattern))
            validations.Add(isChinese ? $"模式: {field.Pattern}" : $"Pattern: {field.Pattern}");
        
        return validations.Count > 0 ? string.Join(", ", validations) : string.Empty;
    }

    /// <summary>
    /// Extracts the fields to display for a given model, handling different model types.
    /// </summary>
    /// <param name="model">The model to extract fields from</param>
    /// <returns>List of fields to display, or empty list if none</returns>
    public static List<ApiField> GetDisplayFields(ApiModel? model)
    {
        if (model == null) return [];

        // Direct fields
        if (model.Fields != null && model.Fields.Count > 0)
            return model.Fields;

        // Tuple elements as fields
        if (model.TupleElements != null && model.TupleElements.Count > 0)
            return model.TupleElements;

        return [];
    }

    /// <summary>
    /// Creates display fields from tuple elements with proper naming.
    /// </summary>
    /// <param name="tupleElements">The tuple elements to convert</param>
    /// <returns>List of fields suitable for display</returns>
    public static List<ApiField> ConvertTupleElementsToFields(List<ApiField>? tupleElements)
    {
        if (tupleElements == null || tupleElements.Count == 0)
            return [];

        return tupleElements.Select((elem, idx) => new ApiField
        {
            Name = elem.Name ?? $"Item{idx + 1}",
            Type = elem.Type,
            Required = elem.Required,
            Description = elem.Description,
            ModelId = elem.ModelId
        }).ToList();
    }
}
