using System.Text;
using System.Xml;
using System.Xml.Serialization;
using OpenApi.Xml.Core.Models;

namespace OpenApi.Xml.Core.Serialization;

public static class ApiDocumentSerializer
{
    private static readonly XmlSerializer Serializer = new(typeof(ApiDocument));

    public static string ToXml(ApiDocument document, bool indented = true, string? encodingName = "utf-8")
    {
        var settings = new XmlWriterSettings
        {
            Indent = indented,
            Encoding = Encoding.GetEncoding(encodingName ?? "utf-8"),
            OmitXmlDeclaration = false
        };
        using var sw = new StringWriterWithEncoding(settings.Encoding);
        using (var writer = XmlWriter.Create(sw, settings))
        {
            Serializer.Serialize(writer, document);
        }
        return sw.ToString();
    }

    public static ApiDocument FromXml(string xml)
    {
        using var reader = new StringReader(xml);
        return (ApiDocument)Serializer.Deserialize(reader)!;
    }

    private sealed class StringWriterWithEncoding : StringWriter
    {
        public override Encoding Encoding { get; }
        public StringWriterWithEncoding(Encoding encoding) => Encoding = encoding;
    }
}
