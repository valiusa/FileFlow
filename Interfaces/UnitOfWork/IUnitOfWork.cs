using Interfaces.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace Interfaces.UnitOfWork
{
    public interface IUnitOfWork
    {
        int SaveChanges();
        Task<int> SaveChangesAsync();
        IRepository<T> GetRepository<T>() where T : class;
        IDbContextTransaction BeginTransaction();
    }
}
