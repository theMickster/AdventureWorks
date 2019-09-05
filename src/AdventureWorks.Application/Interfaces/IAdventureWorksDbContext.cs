using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Application.Interfaces
{
    public interface IAdventureWorksDbContext
    {
        DbSet<Product> Products { get; set; }
    }
}
