using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace app.data
{
    // Standard Repository Interface with CRUD APIs needed for generic entity
    public interface IRepository<T> 
    {
        // Create API
        void Add(T entity);
        void Add(List<T> entities);

        Task AddAsync(T entity);
        Task AddAsync(List<T> entities);


        // Read APIs
        List<T> Get();
        Task<List<T>> GetAsync();

        T GetById(long id);
        Task<T> GetByIdAsync(long id);

        T GetByEntityId(string entityid);
        Task<T> GetByEntityIdAsync(string entityid);

        List<T> Get(Expression<Func<T, bool>> searchPredicate);

        Task<List<T>> GetAsync(Expression<Func<T, bool>> searchPredicate);

        bool Exists(Expression<Func<T, bool>> searchPredicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> searchPredicate);

        long Count();



        // Update APIs
        void Update(T entity);
        void Update(IEnumerable<T> entities);

        Task UpdateAsync(T entity);
        Task UpdateAsync(IEnumerable<T> entities);


        // Delete APIs
        void Delete(long id);
        Task DeleteAsync(long id);    

        void Delete(string entityid);
        Task DeleteAsync(string entityid); 

        long Delete(Expression<Func<T, bool>> predicate);
        Task<long> DeleteAsync(Expression<Func<T, bool>> predicate);

        long DeleteAll();
        Task<long> DeleteAllAsync();

    }

}
