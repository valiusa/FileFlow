using Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly FileFlowDbContext _context;

        public Repository(FileFlowDbContext context)
        {
            _context = context;
        }

        private DbSet<T> DbSet => this._context.Set<T>();

        public void Add(T entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var entry = this._context.Entry(entity);

            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Added;
            }
            else
            {
                this.DbSet.Add(entity);
            }
        }

        public void Attach(T entity)
        {
            this.DbSet.Attach(entity);
        }

        public void Delete(params object[] keyValues)
        {
            var entity = this.Get(keyValues);

            if (entity != null)
            {
                this.Remove(entity);
            }
        }

        public void Detach(T entity)
        {
            var entry = this._context.Entry(entity);
            entry.State = EntityState.Detached;
        }

        public void DetachAllEntyties()
        {
            var changedEntries = _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted ||
                    e.State == EntityState.Unchanged
                )
                .ToList();

            foreach (var entry in changedEntries)
            {
                entry.State = EntityState.Detached;
            }
        }

        public T Get(params object[] keyValues)
        {
            return this.DbSet.Find(keyValues);
        }

        public IQueryable<T> Queryable()
        {
            return this.DbSet.AsQueryable().AsNoTracking();
        }

        public IQueryable<TE> Queryable<TE>() where TE : class
        {
            return _context.Set<TE>().AsQueryable().AsNoTracking();
        }

        public void Remove(T entity)
        {
            var entry = this._context.Entry(entity);

            if (entry.State != EntityState.Deleted)
            {
                entry.State = EntityState.Deleted;
            }
            else
            {
                this.DbSet.Attach(entity);
                this.DbSet.Remove(entity);
            }
        }

        public void Update(T entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var entry = this._context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                this.DbSet.Attach(entity);
            }

            entry.State = EntityState.Modified;
        }
    }
}
