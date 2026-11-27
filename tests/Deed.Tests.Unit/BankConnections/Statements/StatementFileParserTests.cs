using System.Text;
using Deed.Application.BankConnections.Statements;
using FluentAssertions;

namespace Deed.Tests.Unit.BankConnections.Statements;

public sealed class StatementFileParserTests
{
    private const string Iban = "UA213223130000026007233566001";

    private readonly StatementFileParser _parser = new();

    [Fact]
    public void Parse_UkrainianHeadersWithSemicolonsAndDecimalComma_ReadsRows()
    {
        // Arrange
        var csv = "Дата операції;Опис операції;Сума;Валюта;Залишок\r\n"
            + "26.11.2026 14:30:15;Silpo Kyiv;-312,40;UAH;12 840,10\r\n"
            + "25.11.2026 09:00:00;Зарплата;72 000,00;UAH;13 152,50\r\n";

        // Act
        var result = _parser.Parse(Attachment(csv));

        // Assert
        result.Should().NotBeNull();
        result!.Rows.Should().HaveCount(2);
        result.Rows[0].Amount.Should().Be(-312.40m);
        result.Rows[0].Description.Should().Be("Silpo Kyiv");
        result.Rows[0].Currency.Should().Be("UAH");
        result.Rows[0].Balance.Should().Be(12840.10m);
        result.Rows[0].OccurredAt.Should().Be(new DateTimeOffset(2026, 11, 26, 14, 30, 15, TimeSpan.FromHours(2)));
        result.Rows[1].Amount.Should().Be(72000m);
    }

    [Fact]
    public void Parse_EnglishHeadersWithCommasAndIsoDates_ReadsRows()
    {
        // Arrange
        var csv = "Date,Description,Amount,Currency\n"
            + "2026-11-26 14:30:15,Coffee,-45.50,UAH\n"
            + "2026-11-27,Refund,\"1,200.00\",UAH\n";

        // Act
        var result = _parser.Parse(Attachment(csv));

        // Assert
        result!.Rows.Select(r => r.Amount).Should().Equal(-45.50m, 1200m);
    }

    [Fact]
    public void Parse_SplitDebitAndCreditColumns_SignsTheAmount()
    {
        // Arrange
        var csv = "Дата;Призначення платежу;Дебет;Кредит\n"
            + "26.11.2026;Rent;5000,00;\n"
            + "27.11.2026;Salary;;20000,00\n";

        // Act
        var result = _parser.Parse(Attachment(csv));

        // Assert
        result!.Rows.Select(r => r.Amount).Should().Equal(-5000m, 20000m);
    }

    [Fact]
    public void Parse_SeparateTimeColumn_CombinesDateAndTime()
    {
        // Arrange
        var csv = "Дата;Час;Сума\n26.11.2026;08:15;-10,00\n";

        // Act
        var result = _parser.Parse(Attachment(csv));

        // Assert
        result!.Rows.Single().OccurredAt.Should().Be(new DateTimeOffset(2026, 11, 26, 8, 15, 0, TimeSpan.FromHours(2)));
    }

    [Fact]
    public void Parse_KindColumn_AppliesTheDirectionToAPositiveAmount()
    {
        // Arrange
        var csv = "Дата;Тип;Сума\n26.11.2026;Дебет;100,00\n27.11.2026;Кредит;50,00\n";

        // Act
        var result = _parser.Parse(Attachment(csv));

        // Assert
        result!.Rows.Select(r => r.Amount).Should().Equal(-100m, 50m);
    }

    [Fact]
    public void Parse_Windows1251File_DecodesCyrillic()
    {
        // Arrange
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var bytes = Encoding.GetEncoding(1251).GetBytes("Дата;Опис;Сума\r\n26.11.2026;Кав'ярня;-45,00\r\n");

        // Act
        var result = _parser.Parse(new StatementAttachment("statement.csv", "text/csv", bytes));

        // Assert
        result!.Rows.Single().Description.Should().Be("Кав'ярня");
    }

    [Fact]
    public void Parse_Utf8BomFile_IgnoresTheBom()
    {
        // Arrange
        var bytes = new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes("Date;Amount\n26.11.2026;10,00\n")).ToArray();

        // Act
        var result = _parser.Parse(new StatementAttachment("statement.csv", "text/csv", bytes));

        // Assert
        result!.Rows.Should().ContainSingle().Which.Amount.Should().Be(10m);
    }

    [Fact]
    public void Parse_PreambleWithIban_ReturnsTheAccount()
    {
        // Arrange
        var csv = $"Виписка по рахунку {Iban}\nПеріод: листопад\n\nДата;Сума\n26.11.2026;-10,00\n";

        // Act
        var result = _parser.Parse(Attachment(csv));

        // Assert
        result!.Ibans.Should().Equal(Iban);
        result.Rows.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_HtmlTable_ReadsRows()
    {
        // Arrange
        var html = "<html><body><table><tr><th>Дата</th><th>Опис</th><th>Сума</th></tr>"
            + "<tr><td>26.11.2026 10:00</td><td>Bolt<br>Ride</td><td>-129,00</td></tr></table></body></html>";

        // Act
        var result = _parser.Parse(Attachment(html));

        // Assert
        result!.Rows.Single().Description.Should().Be("Bolt Ride");
        result.Rows.Single().Amount.Should().Be(-129m);
    }

    [Fact]
    public void Parse_TotalsAndBlankLines_AreSkipped()
    {
        // Arrange
        var csv = "Дата;Сума\n26.11.2026;-10,00\n\nВсього;-10,00\n";

        // Act
        var result = _parser.Parse(Attachment(csv));

        // Assert
        result!.Rows.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_FileWithoutDateAndAmountColumns_ReturnsNull()
    {
        // Arrange
        var csv = "Name;Value\nfoo;bar\n";

        // Act
        var result = _parser.Parse(Attachment(csv));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Parse_LegacyBinaryExcel_ReturnsNull()
    {
        // Arrange
        var bytes = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

        // Act
        var result = _parser.Parse(new StatementAttachment("statement.xls", "application/vnd.ms-excel", bytes));

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("-1 234,56", -1234.56)]
    [InlineData("1,234.56", 1234.56)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("(100.00)", -100)]
    [InlineData("12,5", 12.5)]
    [InlineData("1,234", 1234)]
    [InlineData("-45.00 UAH", -45)]
    public void ParseAmount_SupportedFormats_ReturnTheValue(string text, double expected)
    {
        // Arrange
        // Act
        var result = StatementValues.ParseAmount(text);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().Be((decimal)expected);
    }

    private static StatementAttachment Attachment(string content)
        => new("statement.csv", "text/csv", Encoding.UTF8.GetBytes(content));
}
