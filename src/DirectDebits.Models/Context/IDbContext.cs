using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace DirectDebits.Models.Context
{
    public interface IDbContext
    {
        int SaveChanges();
        Task<int> SaveChangesAsync();

    }
}