using System.Xml;
using System.Xml.Linq;

namespace Deed.Infrastructure.Services;

internal static class DataProtectionKeyXml
{
    internal static bool IsPlaintextKey(string? xml)
    {
        var root = Parse(xml);

        return root is { Name.LocalName: "key" }
            && root.Descendants().Any(e => e.Name.LocalName == "masterKey" && e.Elements().Any(c => c.Name.LocalName == "value"));
    }

    internal static Guid? KeyId(string? xml)
        => Guid.TryParse(Parse(xml)?.Attribute("id")?.Value, out var id) ? id : null;

    private static XElement? Parse(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return null;
        }

        try
        {
            return XDocument.Parse(xml).Root;
        }
        catch (XmlException)
        {
            return null;
        }
    }
}
