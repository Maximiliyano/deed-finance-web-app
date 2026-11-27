using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Deed.Application.BankConnections.Statements;

internal static partial class StatementTableReader
{
    private const int MaxRows = 100_000;
    private const long MaxEntryBytes = 30_000_000;
    private const int DelimiterSampleLines = 20;

    private static readonly char[] Delimiters = [';', '\t', ',', '|'];
    private static readonly Lazy<Encoding> Windows1251 = new(CreateWindows1251);

    internal static IReadOnlyList<string[]>? Read(StatementAttachment attachment)
    {
        var bytes = attachment.Content;

        if (bytes.Length >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B)
        {
            return ReadXlsx(bytes);
        }

        if (bytes.Length >= 4 && bytes[0] == 0xD0 && bytes[1] == 0xCF && bytes[2] == 0x11 && bytes[3] == 0xE0)
        {
            return null;
        }

        var text = Decode(bytes);

        return text.Contains("<table", StringComparison.OrdinalIgnoreCase)
            ? ReadHtml(text)
            : ReadDelimited(text);
    }

    internal static string Decode(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
        }

        try
        {
            return new UTF8Encoding(false, true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Windows1251.Value.GetString(bytes);
        }
    }

    internal static IReadOnlyList<string[]> ReadDelimited(string text)
    {
        var delimiter = DetectDelimiter(text);
        var rows = new List<string[]>();
        var cells = new List<string>();
        var cell = new StringBuilder();
        var quoted = false;

        var position = 0;

        while (position < text.Length && rows.Count < MaxRows)
        {
            var c = text[position];
            var next = position + 1 < text.Length ? text[position + 1] : '\0';
            var step = 1;

            if (quoted)
            {
                if (c == '"' && next == '"')
                {
                    cell.Append('"');
                    step = 2;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else
                {
                    cell.Append(c);
                }
            }
            else if (c == '"' && cell.Length == 0)
            {
                quoted = true;
            }
            else if (c == delimiter)
            {
                cells.Add(cell.ToString().Trim());
                cell.Clear();
            }
            else if (c is '\r' or '\n')
            {
                step = c == '\r' && next == '\n' ? 2 : 1;
                cells.Add(cell.ToString().Trim());
                cell.Clear();
                rows.Add([.. cells]);
                cells.Clear();
            }
            else
            {
                cell.Append(c);
            }

            position += step;
        }

        if (cell.Length > 0 || cells.Count > 0)
        {
            cells.Add(cell.ToString().Trim());
            rows.Add([.. cells]);
        }

        return rows;
    }

    internal static List<string[]>? ReadHtml(string html)
    {
        List<string[]>? best = null;

        foreach (var segment in TableStart().Split(html).Skip(1))
        {
            var rows = RowPattern().Matches(segment)
                .Select(row => CellPattern().Matches(row.Groups[1].Value)
                    .Select(cell => Clean(cell.Groups[1].Value))
                    .ToArray())
                .Where(cells => cells.Length > 0)
                .Take(MaxRows)
                .ToList();

            if (rows.Count > (best?.Count ?? 1))
            {
                best = rows;
            }
        }

        return best;
    }

    private static List<string[]>? ReadXlsx(byte[] content)
    {
        using var archive = new ZipArchive(new MemoryStream(content), ZipArchiveMode.Read);

        var shared = ReadSharedStrings(archive.GetEntry("xl/sharedStrings.xml"));
        List<string[]>? best = null;

        foreach (var entry in archive.Entries
            .Where(e => SheetEntry().IsMatch(e.FullName) && e.Length <= MaxEntryBytes)
            .OrderBy(e => e.FullName, StringComparer.Ordinal))
        {
            var rows = ReadSheet(entry, shared);

            if (rows.Count > (best?.Count ?? 0))
            {
                best = rows;
            }
        }

        return best;
    }

    private static List<string[]> ReadSheet(ZipArchiveEntry entry, IReadOnlyList<string> shared)
    {
        var rows = new List<string[]>();

        using var stream = entry.Open();
        var document = Load(stream);

        foreach (var row in document.Descendants().Where(e => e.Name.LocalName == "row").Take(MaxRows))
        {
            var cells = new List<string>();

            foreach (var cell in row.Elements().Where(e => e.Name.LocalName == "c"))
            {
                var index = ColumnIndex((string?)cell.Attribute("r"));

                while (cells.Count < index)
                {
                    cells.Add(string.Empty);
                }

                cells.Add(ReadCell(cell, shared));
            }

            rows.Add([.. cells]);
        }

        return rows;
    }

    private static string ReadCell(XElement cell, IReadOnlyList<string> shared)
    {
        var type = (string?)cell.Attribute("t");

        if (type == "inlineStr")
        {
            return string.Concat(cell.Descendants().Where(e => e.Name.LocalName == "t").Select(e => e.Value)).Trim();
        }

        var value = cell.Elements().FirstOrDefault(e => e.Name.LocalName == "v")?.Value ?? string.Empty;

        return type == "s" && int.TryParse(value, out var index) && index >= 0 && index < shared.Count
            ? shared[index].Trim()
            : value.Trim();
    }

    private static List<string> ReadSharedStrings(ZipArchiveEntry? entry)
    {
        if (entry is null || entry.Length > MaxEntryBytes)
        {
            return [];
        }

        using var stream = entry.Open();

        return [.. Load(stream).Descendants()
            .Where(e => e.Name.LocalName == "si")
            .Select(si => string.Concat(si.Descendants().Where(e => e.Name.LocalName == "t" && e.Parent?.Name.LocalName != "rPh").Select(e => e.Value)))];
    }

    private static XDocument Load(Stream stream)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = 200_000_000
        };

        using var reader = XmlReader.Create(stream, settings);

        return XDocument.Load(reader);
    }

    private static int ColumnIndex(string? reference)
    {
        var index = 0;

        foreach (var c in reference ?? string.Empty)
        {
            if (!char.IsAsciiLetter(c))
            {
                break;
            }

            index = index * 26 + char.ToUpperInvariant(c) - 'A' + 1;
        }

        return Math.Max(index - 1, 0);
    }

    private static char DetectDelimiter(string text)
    {
        var lines = text.Split('\n').Where(line => line.Trim().Length > 0).Take(DelimiterSampleLines).ToList();
        var best = ',';
        var bestScore = 0;

        foreach (var delimiter in Delimiters)
        {
            var counts = lines.Select(line => CountOutsideQuotes(line, delimiter)).Where(count => count > 0).ToList();

            if (counts.Count == 0)
            {
                continue;
            }

            var mode = counts.GroupBy(count => count).OrderByDescending(group => group.Count()).First().Key;
            var score = counts.Count(count => count == mode) * 100 + mode;

            if (score > bestScore)
            {
                bestScore = score;
                best = delimiter;
            }
        }

        return best;
    }

    private static int CountOutsideQuotes(string line, char delimiter)
    {
        var count = 0;
        var quoted = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                quoted = !quoted;
            }
            else if (c == delimiter && !quoted)
            {
                count++;
            }
        }

        return count;
    }

    private static string Clean(string html)
        => StatementValues.Collapse(WebUtility.HtmlDecode(TagPattern().Replace(BreakPattern().Replace(html, " "), " ")));

    private static Encoding CreateWindows1251()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        return Encoding.GetEncoding(1251);
    }

    [GeneratedRegex(@"<table\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 2000)]
    private static partial Regex TableStart();

    [GeneratedRegex(@"<tr\b[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant, 2000)]
    private static partial Regex RowPattern();

    [GeneratedRegex(@"<t[dh]\b[^>]*>(.*?)</t[dh]>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant, 2000)]
    private static partial Regex CellPattern();

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 2000)]
    private static partial Regex BreakPattern();

    [GeneratedRegex(@"<[^>]+>", RegexOptions.CultureInvariant, 2000)]
    private static partial Regex TagPattern();

    [GeneratedRegex(@"^xl/worksheets/sheet\d+\.xml$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 2000)]
    private static partial Regex SheetEntry();
}
