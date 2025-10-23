using System.Reflection;
using System.Xml.Linq;

namespace AspNetCore.OpenApi.Xml.Services;

/// <summary>
/// Service to read XML documentation comments from assembly XML files
/// </summary>
public interface IXmlDocumentationReader
{
    /// <summary>
    /// Get the summary comment for a method
    /// </summary>
    /// <param name="methodInfo">The method to get documentation for</param>
    /// <returns>The summary text or null if not found</returns>
    string? GetMethodSummary(MethodInfo methodInfo);
    
    /// <summary>
    /// Load XML documentation for an assembly
    /// </summary>
    /// <param name="assembly">The assembly to load documentation for</param>
    void LoadXmlDocumentation(Assembly assembly);
}

public class XmlDocumentationReader : IXmlDocumentationReader
{
    private readonly Dictionary<string, XElement> _documentation = new();

    public void LoadXmlDocumentation(Assembly assembly)
    {
        try
        {
            var xmlPath = Path.ChangeExtension(assembly.Location, ".xml");
            if (!File.Exists(xmlPath))
                return;

            var doc = XDocument.Load(xmlPath);
            var members = doc.Descendants("member");
            
            foreach (var member in members)
            {
                var nameAttr = member.Attribute("name");
                if (nameAttr != null)
                {
                    _documentation[nameAttr.Value] = member;
                }
            }
        }
        catch
        {
            // Silently ignore XML documentation loading errors
        }
    }

    public string? GetMethodSummary(MethodInfo methodInfo)
    {
        var memberName = GetMemberName(methodInfo);
        if (memberName != null && _documentation.TryGetValue(memberName, out var element))
        {
            var summary = element.Element("summary");
            if (summary != null)
            {
                return CleanupDocumentation(summary.Value);
            }
        }
        return null;
    }

    private static string? GetMemberName(MethodInfo methodInfo)
    {
        var declaringType = methodInfo.DeclaringType;
        if (declaringType == null)
            return null;

        var parameters = methodInfo.GetParameters();
        var parameterTypes = parameters.Select(p => GetTypeName(p.ParameterType));
        var parameterList = parameters.Length > 0 ? $"({string.Join(",", parameterTypes)})" : "";
        
        return $"M:{declaringType.FullName}.{methodInfo.Name}{parameterList}";
    }

    private static string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            var genericArgs = type.GetGenericArguments();
            var genericTypeName = genericType.FullName?.Split('`')[0] ?? genericType.Name.Split('`')[0];
            var genericArgsString = string.Join(",", genericArgs.Select(GetTypeName));
            return $"{genericTypeName}{{{genericArgsString}}}";
        }
        
        return type.FullName ?? type.Name;
    }

    private static string CleanupDocumentation(string text)
    {
        // Remove extra whitespace and trim
        var lines = text.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line));
        
        return string.Join(" ", lines);
    }
}
