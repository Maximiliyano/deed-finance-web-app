using System.Text.Json.Serialization;
using Deed.Domain.Converters;

namespace Deed.Domain.Entities;

public sealed class Exchange
    : Entity, IAuditableEntity, ISoftDeletableEntity
{
    public Exchange(int id)
        : base(id)
    {
    }

    public Exchange()
    {
    }

    [JsonPropertyName("base_ccy")]
    public required string NationalCurrencyCode { get; init; }

    [JsonPropertyName("ccy")]
    public required string TargetCurrencyCode { get; init; }

    [JsonConverter(typeof(StringTofloatConverter))]
    public required decimal Buy { get; set; }

    [JsonConverter(typeof(StringTofloatConverter))]
    public required decimal Sale { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public string? CreatedBy { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public string? UpdatedBy { get; init; }

    public bool? IsDeleted { get; set; }
}
