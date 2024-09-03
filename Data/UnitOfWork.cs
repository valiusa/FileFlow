using Interfaces.Repository;
using Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConcurrentDictionary<Type, object> repositoryPool = new ConcurrentDictionary<Type, object>();
        private readonly FileFlowDbContext context;

        public UnitOfWork(FileFlowDbContext context)
        {
            this.context = context;
        }

        public IDbContextTransaction BeginTransaction()
        {
            return this.context.Database.BeginTransaction();
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);

            return (IRepository<T>)this.repositoryPool.GetOrAdd(type, t => new Repository<T>(this.context));
        }

        public int SaveChanges()
        {
            return this.context.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            return this.context.SaveChangesAsync();
        }
    }
}
