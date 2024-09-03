namespace Interfaces.Repository
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);
        void Update(T entity);
        void Delete(params object[] keyValues);
        void Remove(T entity);
        void Attach(T entity);
        void Detach(T entity);
        void DetachAllEntyties();

        T Get(params object[] keyValues);
        IQueryable<T> Queryable();
        IQueryable<TE> Queryable<TE>() where TE : class;
    }
}
