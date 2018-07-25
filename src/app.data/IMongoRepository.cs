using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;

using app.model;

namespace app.data
{
    public interface IMongoRepository<T>: IEvented<T>, IRepository<T> where T : IDbEntity<T>
    {

        void Setup(string connection, string db, string collection);

        T GetByEntityId(ObjectId entityid);
        Task<T> GetByEntityIdAsync(ObjectId entityid);

        void Delete(ObjectId entityid);
        Task DeleteAsync(ObjectId entityid);  

    }
}
