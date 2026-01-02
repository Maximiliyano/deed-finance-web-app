using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Domain.Entities;

namespace Deed.Domain.Repositories;

public interface ITagRepository
{
    void Create(Tag tag);

    void Delete(Tag tag);

    Task<Tag?> GetAsync(ISpecification<Tag> specification);

    Task<bool> AnyAsync(ISpecification<Tag> specification);
}
