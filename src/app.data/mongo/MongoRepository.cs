using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Driver.Linq;

using app.model;
using System.Runtime.Serialization;


namespace app.data.mongo
{
    // Repository for MongoDB 
    public class MongoRepository<T> : EventedRepository<T>, IMongoRepository<T>  where T: IDbEntity<T>
    {
        private readonly ILogger<MongoRepository<T>> _logger;

        MongoClient _dbClient;
        IMongoDatabase _db;
        IMongoCollection<T> _collection;

        public MongoRepository(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for object initialization");
            
            this._logger = loggerFactory.CreateLogger<MongoRepository<T>>();
        }

        // Call Setup to setup the server, database and collection information
        public void Setup(string connectionString, string db, string collection)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Setting up Mongo repository. ConnectionStr:{connectionString} Database:{db} - Collection:{collection}");

            this._dbClient = new MongoClient(connectionString); 

            this._db = _dbClient.GetDatabase(db);

            this._collection = _db.GetCollection<T>(collection);

            _logger.LogTrace(LoggingEvents.Trace, $"Repository Setup completed. Database:{db} - Collection:{collection}");
        }

        public void Add(T entity)
        {            
            this.AddAsync(entity).Wait();
        }

        public async Task AddAsync(T entity)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Adding entity");

            entity.updatedDate = DateTime.UtcNow;
            await this._collection.InsertOneAsync(entity);
            AppEventArgs<T> e = new AppEventArgs<T>();
            e.appEventType = AppEventType.Insert;
            e.afterChange = entity;
            this.OnEvent(AppEventType.Insert, e);
            this.OnAnyEvent(e);
        }

        public void Add(List<T> entities)
        {
            this.AddAsync(entities).Wait();
        }

        public async Task AddAsync(List<T> entities)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Adding {entities.Count} entities");

            entities.ForEach(c => c.updatedDate = DateTime.UtcNow);
            await this._collection.InsertManyAsync(entities);           
            entities.ForEach(c => {
                AppEventArgs<T> e = new AppEventArgs<T>();
                e.appEventType = AppEventType.Insert;
                e.afterChange = c;
                this.OnEvent(AppEventType.Insert, e);
                this.OnAnyEvent(e);

            });
        }

        public List<T> Get()
        {
            return this.GetAsync().Result;
        }

        public async Task<List<T>> GetAsync()
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Getting all entities");

            var filter = Builders<T>.Filter.Eq("deleted", false);
            return await this._collection.Find(filter).ToListAsync();
        }

        public List<T> Get(Expression<Func<T, bool>> searchPredicate){
            return this.GetAsync(searchPredicate).Result;
        }

        public async Task<List<T>> GetAsync(Expression<Func<T, bool>> searchPredicate)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Getting entity that match defined search criteria");

            var filter = Builders<T>.Filter.And(
                                searchPredicate,
                                Builders<T>.Filter.Eq("deleted", false)
                        );

            return await this._collection.Find(filter).ToListAsync();
        }

        public T GetById(long id)
        {
            return this.GetByIdAsync(id).Result;
        }

        public async Task<T> GetByIdAsync(long id)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Getting entity id:{id}");

            var filter = Builders<T>.Filter.And(
                                Builders<T>.Filter.Eq("id", id), 
                                Builders<T>.Filter.Eq("deleted", false)
                        );

            var result = await this._collection.Find(filter).ToListAsync();

            if (result.Count > 0){
                return result[0];
            }

            throw new Exception($"Could not find object with id:{id}"); 
        }

        public T GetByEntityId(string entityid)
        {
            return this.GetByEntityId(new ObjectId(entityid));
        }

        public T GetByEntityId(ObjectId entityid)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Getting entity with entityid:{entityid.ToString()}");

            var filter = Builders<T>.Filter.And(
                                Builders<T>.Filter.Eq("entityid", entityid), 
                                Builders<T>.Filter.Eq("deleted", false)
                        );

            var result = this._collection.Find(filter).ToList();

            if (result.Count > 0){
                return result[0];
            }

            throw new Exception($"Could not find object with entityid:{entityid}"); 
        }

        public async Task<T> GetByEntityIdAsync(string entityid)
        {
            return await this.GetByEntityIdAsync(new ObjectId(entityid));
        }
        public async Task<T> GetByEntityIdAsync(ObjectId entityid)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Getting entity with entityid:{entityid.ToString()}");

            var filter = Builders<T>.Filter.And(
                                Builders<T>.Filter.Eq("entityid", entityid), 
                                Builders<T>.Filter.Eq("deleted", false)
                        );

            var result = await this._collection.Find(filter).ToListAsync();

            if (result.Count > 0){
                return result[0];
            }

            throw new Exception($"Could not find object with entityid:{entityid}"); 
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return this.ExistsAsync(predicate).Result;
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Checking for entity that matches given predicate");

            var result = await this._collection.AsQueryable<T>()
                .Where(e => e.deleted == false)
                .Where(predicate)            
                .FirstOrDefaultAsync();

            if(result != null){
                return true;
            }

            return false;
        }

        public long Count()
        {
            return this.CountAsync().Result;
        }

        public async Task<long> CountAsync()
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Getting entity count");

            var filter = Builders<T>.Filter.Eq("deleted", false);

            var count = await this._collection.CountDocumentsAsync(filter);
            return count;
        }


        public void Update(T entity) {
            this.UpdateAsync(entity).Wait();
        }

        public void Update(IEnumerable<T> entities) {
            this.UpdateAsync(entities).Wait();
        }

        public async Task UpdateAsync(T entity) {

            _logger.LogTrace(LoggingEvents.Trace, $"U entitpdatingy with entityid:{entity.entityid.ToString()}");

            AppEventArgs<T> e = new AppEventArgs<T>();
            e.appEventType = AppEventType.Update;
            
            e.beforeChange = await this.GetByEntityIdAsync(entity.entityid);

            entity.updatedDate = DateTime.UtcNow;
            var filter = Builders<T>.Filter.Eq("entityid", entity.entityid);

            var result = await this._collection.FindOneAndReplaceAsync(filter, entity);
            e.afterChange = entity;
            this.OnEvent(AppEventType.Update, e);
            this.OnAnyEvent(e);

        }

        public async Task UpdateAsync(IEnumerable<T> entities)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Bulk updating list of entities");

            foreach (T entity in entities)
            {
                await this.UpdateAsync(entity);
            }
        }

        public void Delete(string entityid)
        {
            this.DeleteAsync( new ObjectId(entityid)).Wait();
        }

        public void Delete(ObjectId entityid)
        {
            this.DeleteAsync(entityid).Wait();
        }

        public async Task DeleteAsync(String entityid)
        {
            await DeleteAsync(new ObjectId(entityid));
        }

        public async Task DeleteAsync(ObjectId entityid)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Deleting entity with entityid:{entityid.ToString()}");

            AppEventArgs<T> e = new AppEventArgs<T>();
            e.appEventType = AppEventType.Delete;
            
            e.beforeChange = await this.GetByEntityIdAsync(entityid);
            e.afterChange = e.beforeChange.Clone() .Clone();
            var updateTimestamp = DateTime.UtcNow;
            e.afterChange.deleted = true;
            e.afterChange.updatedDate = updateTimestamp;

             var filter = Builders<T>.Filter.Eq("entityid", entityid);
             var update = Builders<T>.Update
                .Set("deleted", true)
                .Set("updatedDate", updateTimestamp);

            var result = await _collection.UpdateOneAsync(filter, update);


            _logger.LogDebug($"{result.MatchedCount} records matched. {result.ModifiedCount} records updated");

            this.OnEvent(AppEventType.Delete, e);
            this.OnAnyEvent(e);
            return;
        }

        public long Delete(Expression<Func<T, bool>> predicate)
        {
            return this.DeleteAsync(predicate).Result;
        }

        public async Task<long> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Deleting entities matching specified criteria");

            var updateTimestamp = DateTime.UtcNow;

            var entities = await this.GetAsync(predicate);

            var filter = Builders<T>.Filter.And(
                                predicate,
                                Builders<T>.Filter.Eq("deleted", false)
                        );

             var update = Builders<T>.Update
                .Set("deleted", true)
                .Set("updatedDate", updateTimestamp);

            var result = await this._collection.UpdateManyAsync(filter, update);   

            if(result.ModifiedCount != entities.Count){
                var msg = $"Not all items could be deleted - Criteria matched for:{entities.Count} ... however only {result.ModifiedCount} were deleted";
                this._logger.LogWarning(msg);
                throw new Exception(msg);
            }

            foreach(var entity in entities){
                AppEventArgs<T> e = new AppEventArgs<T>();
                e.appEventType = AppEventType.Delete;

                e.beforeChange = entity;
                e.afterChange = e.beforeChange.Clone();
                e.afterChange.updatedDate = updateTimestamp;
                e.afterChange.deleted = true;

                this.OnEvent(AppEventType.Delete, e);
                this.OnAnyEvent(e);
            }

            return result.ModifiedCount;             

        }


        public long DeleteAll()
        {
            return this.DeleteAllAsync().Result;
        }

        public async Task<long> DeleteAllAsync()
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Deleting all entities");

            var updateTimestamp = DateTime.UtcNow;

            var entities = await this.GetAsync();

            var filter = Builders<T>.Filter.Eq("deleted", false);

             var update = Builders<T>.Update
                .Set("deleted", true)
                .Set("updatedDate", updateTimestamp);

            var result = await this._collection.UpdateManyAsync(filter, update);   
            foreach(var entity in entities){
                AppEventArgs<T> e = new AppEventArgs<T>();
                e.appEventType = AppEventType.Delete;

                e.beforeChange = entity;
                e.afterChange = e.beforeChange.Clone();
                e.afterChange.updatedDate = updateTimestamp;
                e.afterChange.deleted = true;

                this.OnEvent(AppEventType.Delete, e);
                this.OnAnyEvent(e);
            }

            return result.ModifiedCount;         
                        
        }


        public void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }
    }
}
