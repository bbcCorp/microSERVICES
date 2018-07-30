using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using app.model;
using Nest;

namespace app.data.elastic
{
    // Standard ElasticSearch Repository Interface with CRUDS APIs needed for generic entity
    public interface IElasticRepository<T>  where T : class, IEntity<T>
    {
        // Create API
        void Add(T entity);
        void Add(List<T> entities);

        Task AddAsync(T entity);
        Task AddAsync(List<T> entities);


        // Search APIs
        ISearchResponse<T> Search(QueryContainer query, int skip=0, int take=50);
        Task<ISearchResponse<T>> SearchAsync(QueryContainer query, int skip=0, int take=50);


        // Read APIs
        T GetById(long id);
        Task<T> GetByIdAsync(long id);

        bool Exists(long id);
        Task<bool> ExistsAsync(long id);

        // bool Exists(Expression<Func<T, Boolean>> searchPredicate);
        // Task<bool> ExistsAsync(Expression<Func<T, bool>> searchPredicate);

        long Count();


        // Update APIs
        void Update(T entity);
        void Update(IEnumerable<T> entities);

        Task UpdateAsync(T entity);
        Task UpdateAsync(IEnumerable<T> entities);


        // Delete APIs
        void Delete(long id);
        void Delete(IList<long> ids);

        Task DeleteAsync(long id);    
        Task DeleteAsync(IList<long> ids);

        // long DeleteAll();
        // Task<long> DeleteAllAsync();

    }

}
