namespace AspNetCore.OpenApi.Xml.Components;

/// <summary>
/// Service for localizing UI strings.
/// </summary>
public static class LocalizationService
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        ["zh-CN"] = new()
        {
            ["version"] = "版本",
            ["apiDoc"] = "API 文档",
            ["selectEndpoint"] = "请从左侧选择一个接口以查看详细信息",
            ["searchEndpoints"] = "搜索接口...",
            ["routeParams"] = "路由参数",
            ["queryParams"] = "查询参数",
            ["headers"] = "请求头",
            ["requestBody"] = "请求体",
            ["responses"] = "响应",
            ["noParams"] = "无参数",
            ["paramName"] = "参数名",
            ["type"] = "类型",
            ["required"] = "必填",
            ["optional"] = "可选",
            ["description"] = "说明",
            ["noResponse"] = "无响应体",
            ["typeInfo"] = "类型信息",
            ["namespace"] = "命名空间",
            ["nullable"] = "可空",
            ["yes"] = "是",
            ["enumValues"] = "枚举值",
            ["fields"] = "字段",
            ["elementType"] = "元素类型",
            ["dictionaryType"] = "字典类型",
            ["keyType"] = "键类型",
            ["valueType"] = "值类型",
            ["tupleType"] = "元组类型",
            ["tupleElements"] = "元组元素",
            ["genericType"] = "泛型类型",
            ["genericParams"] = "泛型参数",
            ["name"] = "名称",
            ["arrayType"] = "数组类型",
            ["modelNotFound"] = "模型未找到",
            ["responseExample"] = "响应示例",
            ["codePath"] = "代码路径"
        },
        ["en-US"] = new()
        {
            ["version"] = "Version",
            ["apiDoc"] = "API Documentation",
            ["selectEndpoint"] = "Please select an endpoint from the left to view details",
            ["searchEndpoints"] = "Search endpoints...",
            ["routeParams"] = "Route Parameters",
            ["queryParams"] = "Query Parameters",
            ["headers"] = "Headers",
            ["requestBody"] = "Request Body",
            ["responses"] = "Responses",
            ["noParams"] = "No parameters",
            ["paramName"] = "Parameter",
            ["type"] = "Type",
            ["required"] = "Required",
            ["optional"] = "Optional",
            ["description"] = "Description",
            ["noResponse"] = "No response body",
            ["typeInfo"] = "Type Information",
            ["namespace"] = "Namespace",
            ["nullable"] = "Nullable",
            ["yes"] = "Yes",
            ["enumValues"] = "Enum Values",
            ["fields"] = "Fields",
            ["elementType"] = "Element Type",
            ["dictionaryType"] = "Dictionary Type",
            ["keyType"] = "Key Type",
            ["valueType"] = "Value Type",
            ["tupleType"] = "Tuple Type",
            ["tupleElements"] = "Tuple Elements",
            ["genericType"] = "Generic Type",
            ["genericParams"] = "Generic Parameters",
            ["name"] = "Name",
            ["arrayType"] = "Array Type",
            ["modelNotFound"] = "Model not found",
            ["responseExample"] = "Response Example",
            ["codePath"] = "Code Path"
        }
    };

    /// <summary>
    /// Localizes a key to the specified language.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="language">The language code (e.g., "zh-CN", "en-US").</param>
    /// <returns>The localized string.</returns>
    public static string Localize(string key, string language)
    {
        if (Translations.TryGetValue(language, out var langDict) && langDict.TryGetValue(key, out var value))
        {
            return value;
        }
        
        // Fallback to English
        if (Translations.TryGetValue("en-US", out var enDict) && enDict.TryGetValue(key, out var enValue))
        {
            return enValue;
        }

        return key;
    }
}
