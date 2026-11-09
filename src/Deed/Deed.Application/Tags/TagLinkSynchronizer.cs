using Deed.Application.Tags.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;

namespace Deed.Application.Tags;

internal static class TagLinkSynchronizer
{
    public static List<string> Normalize(IEnumerable<string> names) =>
        [.. names.Select(n => n.Trim()).Where(n => n.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase)];

    public static bool HasSameTags<TLink>(IEnumerable<TLink> links, IReadOnlyCollection<string> desired)
        where TLink : ITagLink
        => links
            .Where(l => !l.IsDeleted)
            .Select(l => l.Tag.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            .SetEquals(desired);

    public static async Task SyncAsync<TLink>(
        ICollection<TLink> links,
        IReadOnlyCollection<string> desired,
        ITagRepository tagRepository,
        Func<Tag, TLink> createLink,
        CancellationToken cancellationToken)
        where TLink : ITagLink
    {
        var desiredSet = desired.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var link in links.Where(l => !l.IsDeleted && !desiredSet.Contains(l.Tag.Name)).ToList())
        {
            link.IsDeleted = true;
        }

        var activeNames = links
            .Where(l => !l.IsDeleted)
            .Select(l => l.Tag.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = desired.Where(name => !activeNames.Contains(name)).ToList();

        if (missing.Count == 0)
        {
            return;
        }

        var existing = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in await tagRepository
                     .GetAllAsync(new TagsByNamesSpecification(missing, tracking: true), cancellationToken)
                     .ConfigureAwait(false))
        {
            existing[tag.Name] = tag;
        }

        foreach (var name in missing)
        {
            if (!existing.TryGetValue(name, out var tag))
            {
                links.Add(createLink(new Tag { Name = name }));
                continue;
            }

            var revived = links.FirstOrDefault(l => l.IsDeleted && l.TagId == tag.Id);

            if (revived is not null)
            {
                revived.IsDeleted = false;
                continue;
            }

            links.Add(createLink(tag));
        }
    }
}
