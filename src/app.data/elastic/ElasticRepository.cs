using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Nest;
using app.model;
using Elasticsearch.Net;

namespace app.data.elastic
{
    public class ElasticRepository<T> : IElasticRepository<T> where T : class,IEntity<T> 
    {
        private readonly ILogger<ElasticRepository<T>> _logger;

        private ElasticClient _esClient;
        private readonly string _entityType;
        private readonly string _entityIndex;

        public ElasticRepository(ILoggerFactory loggerFactory, string serverUrl = "", string[] serverUrlLst = null, 
                string defaultTypeName="", string defaultIndex="search")
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for object initialization");
            
            if( String.IsNullOrEmpty(serverUrl) && (serverUrlLst == null || serverUrlLst.Count() > 0))
            {
                throw new ArgumentException("serverUrl needs to be specified");
            }

            this._logger = loggerFactory.CreateLogger<ElasticRepository<T>>();

            if(String.IsNullOrEmpty(defaultTypeName)){
                this._entityType = typeof(T).Name.ToLower();
            }
            else {
                this._entityType = defaultTypeName;
            }

            this._entityIndex = defaultIndex;

            ConnectionSettings settings;

            if(serverUrlLst != null && serverUrlLst.Count() > 0){
                List<Uri> uris = new List<Uri>();
                Array.ForEach(serverUrlLst, url => uris.Add(new Uri(url)));

                var connectionPool = new SniffingConnectionPool(uris);
                settings = new ConnectionSettings(connectionPool)
                            .RequestTimeout(TimeSpan.FromMinutes(2))
                            .DefaultTypeName(this._entityType)
                            .DefaultIndex(this._entityIndex);
            }
            else {
                settings = new ConnectionSettings(new Uri(serverUrl))
                            .RequestTimeout(TimeSpan.FromMinutes(2))
                            .DefaultTypeName(this._entityType)
                            .DefaultIndex(this._entityIndex);
            }


            this._esClient = new ElasticClient(settings);
        }

        // CREATE APIs
        public void Add(T entity)
        {
            var result = this._esClient.IndexDocument<T>(entity);


            if(! result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in added entity of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Added entity ID {result.Id} of type {this._entityType} to index {this._entityIndex}", new[] { result} );

        }

        public async Task AddAsync(T entity)
        {
            var result = await this._esClient.IndexDocumentAsync<T>(entity);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in added entity of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }
            
            this._logger.LogTrace($"Added entity ID {result.Id} of type {this._entityType} to index {this._entityIndex}", new[] { result} );
            
        }

        public void Add(List<T> entities)
        {
            IBulkResponse result = this._esClient.IndexMany<T>(entities,this._entityIndex, this._entityType);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in adding entities of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Added {result.Items.Count} entity of type {this._entityType} to index {this._entityIndex}. Time taken: {result.Took}", new[] { result } );

        }

        public async Task AddAsync(List<T> entities)
        {
            IBulkResponse result = await this._esClient.IndexManyAsync<T>(entities,this._entityIndex, this._entityType);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in adding entities of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Added {result.Items.Count} entity of type {this._entityType} to index {this._entityIndex}. Time taken: {result.Took}", new[] { result } );

        }
        
        // Search APIs
        public ISearchResponse<T> Search(QueryContainer query, int skip=0, int take=50)
        {
            var searchRequest = new SearchRequest
            {
                From = skip,
                Size = take,
                Query = query
            };

            ISearchResponse<T> result = this._esClient.Search<T>(searchRequest);
            
            this._logger.LogTrace($"Search matched {result.Total} entity from type {this._entityType} - index {this._entityIndex}.", new[] { result } ); 

            return result;
        }  

        public async Task<ISearchResponse<T>> SearchAsync(QueryContainer query, int skip=0, int take=50)
        {
            var searchRequest = new SearchRequest
            {
                From = skip,
                Size = take,
                Query = query
            };

            ISearchResponse<T> result = await this._esClient.SearchAsync<T>(searchRequest);
            
            this._logger.LogTrace($"Search matched {result.Total} entity from type {this._entityType} - index {this._entityIndex}.", new[] { result } ); 

            return result;
        }        



        // Read APIs

        public bool Exists(long id)
        {
            IExistsResponse result = this._esClient.DocumentExists<T>(id, d => d
                .Index(this._entityIndex)
                .Type(this._entityType));

            if(! result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error deleting entities of type {this._entityType} from index {this._entityIndex}.", new[] { result } );
                throw result.OriginalException;
            }

            return result.Exists;

        }

        public async Task<bool> ExistsAsync(long id)
        {
            IExistsResponse result = await this._esClient.DocumentExistsAsync<T>(id, d => d
                .Index(this._entityIndex)
                .Type(this._entityType));

            if(! result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error deleting entities of type {this._entityType} from index {this._entityIndex}.", new[] { result } );
                throw result.OriginalException;
            }

            return result.Exists;
        }

        public T GetById(long id)
        {
            List<T> entities = new List<T>();
            IGetResponse<T> result = this._esClient.Get<T>(id);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in getting entity: {id} of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Retrieved entity {result.Id} from type {this._entityType} - index {this._entityIndex}.", new[] { result } ); 

            return result.Source;
        }

        public async Task<T> GetByIdAsync(long id)
        {
            List<T> entities = new List<T>();
            IGetResponse<T> result = await this._esClient.GetAsync<T>(id);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in getting entity: {id} of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Retrieved entity {result.Id} from type {this._entityType} - index {this._entityIndex}.", new[] { result } ); 

            return result.Source;
        }


        public List<T> GetById(long[] ids)
        {
            List<T> entities = new List<T>();
            IEnumerable<IMultiGetHit<T>> results = this._esClient.GetMany<T>(ids, this._entityIndex, this._entityType);

            foreach(var r in results)
            {
                entities.Add(r.Source);
            }

            this._logger.LogTrace($"Retrieved {entities.Count} entities from type {this._entityType} - index {this._entityIndex}."); 


            return entities;
        }

        public async Task<List<T>> GetByIdAsync(long[] ids)
        {
            List<T> entities = new List<T>();
            IEnumerable<IMultiGetHit<T>> results = await this._esClient.GetManyAsync<T>(ids, this._entityIndex, this._entityType);

            foreach(var r in results)
            {
                entities.Add(r.Source);
            }

            this._logger.LogTrace($"Retrieved {entities.Count} entities from type {this._entityType} - index {this._entityIndex}."); 


            return entities;
        }

        public long Count()
        {
            ICountResponse result = this._esClient.Count<T>(
                c =>    c.Index(this._entityIndex)
                        .Type(this._entityType) 
            );

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in getting count of entities of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }            

            this._logger.LogTrace($"There are {result.Count} entities of type {this._entityType} in index {this._entityIndex}.", new[] { result } );

            return result.Count;
        }


        // Update APIs
        public void Update(T entity)
        {
            var result = this._esClient.IndexDocument<T>(entity);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in updating entity of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Updated entity ID {result.Id} of type {this._entityType} to index {this._entityIndex}", new[] { result} );

        }

        public void Update(IEnumerable<T> entities)
        {
            IBulkResponse result = this._esClient.IndexMany<T>(entities);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in updating entities of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Updated {result.Items.Count} entities of type {this._entityType} to index {this._entityIndex}", new[] { result} );
           
        }

        public async Task UpdateAsync(T entity)
        {
            var result = await this._esClient.IndexDocumentAsync<T>(entity);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in updating entity of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Updated entity ID {result.Id} of type {this._entityType} to index {this._entityIndex}", new[] { result} );       

        }

        public async Task UpdateAsync(IEnumerable<T> entities)
        {
            IBulkResponse result = await this._esClient.IndexManyAsync<T>(entities);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error in updating entities of type {this._entityType} to index {this._entityIndex}" , new[] { result} );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Updated {result.Items.Count} entities of type {this._entityType} to index {this._entityIndex}", new[] { result} );

        }


        // Delete APIs
        public void Delete(long id)
        {
            DeleteRequest request = new DeleteRequest(this._entityIndex, this._entityType , id);
            IDeleteResponse result = this._esClient.Delete(request);
            
            this._logger.LogTrace($"Deleted entity:{result.Id} of type {this._entityType} from index {this._entityIndex}.", new[] { result } );

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error deleting entity {id} of type {this._entityType} from index {this._entityIndex}.", new[] { result } );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Deleted entity:{result.Id} of type {this._entityType} from index {this._entityIndex}.", new[] { result } );
        }

        public async Task DeleteAsync(long id)
        {
            DeleteRequest request = new DeleteRequest(this._entityIndex, this._entityType , id);
            IDeleteResponse result = await this._esClient.DeleteAsync(request);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error deleting entity {id} of type {this._entityType} from index {this._entityIndex}.", new[] { result } );
                throw result.OriginalException;
            }

            this._logger.LogTrace($"Deleted entity:{result.Id} of type {this._entityType} from index {this._entityIndex}.", new[] { result } );          
        }   

        public void Delete(IList<long> ids)
        {
            var descriptor = new BulkDescriptor();

            foreach (var id in ids){
                descriptor.Delete<T>(x => x
                    .Id(id))
                    .Refresh(Refresh.WaitFor);
            }

            IBulkResponse result = this._esClient.Bulk(descriptor);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error deleting entities of type {this._entityType} from index {this._entityIndex}.", new[] { result } );
                throw result.OriginalException;
            }            

            this._logger.LogTrace($"Deleted {result.Items.Count} entities of type {this._entityType} from index {this._entityIndex}.", new[] { result } );

        }

        public async Task DeleteAsync(IList<long> ids)
        {
            var descriptor = new BulkDescriptor();

            foreach (var id in ids){
                descriptor.Delete<T>(x => x
                    .Id(id))
                    .Refresh(Refresh.WaitFor);
            }

            IBulkResponse result = await this._esClient.BulkAsync(descriptor);

            if(!result.IsValid)
            {
                this._logger.LogError(result.OriginalException, $"Error deleting entities of type {this._entityType} from index {this._entityIndex}.", new[] { result } );
                throw result.OriginalException;
            }            

            this._logger.LogTrace($"Deleted {result.Items.Count} entities of type {this._entityType} from index {this._entityIndex}.", new[] { result } );

        }

        // public long DeleteAll()
        // {
        //     var request = new DeleteByQueryRequest(new[] { this._entityIndex }, new[] { this._entityType });
        //     request.Query = new QueryContainer( new MatchAllQuery {  });
        //     request.Routing = "nest";

        //     IDeleteByQueryResponse result = this._esClient.DeleteByQuery(request);

        //     this._logger.LogTrace($"Deleted {result.Deleted} entities of type {this._entityType} from index {this._entityIndex}.", new[] { result } ); 

        //     return result.Deleted;
        // }

        // public async Task<long> DeleteAllAsync()
        // {
        //     var request = new DeleteByQueryRequest(new[] { this._entityIndex }, new[] { this._entityType });
        //     request.Query = new QueryContainer( new MatchAllQuery {  });
        //     request.Routing = "nest";

        //     var result = await this._esClient.DeleteByQueryAsync(request);
        //     return result.Deleted;
        // }


        #region "Not implemented"


        #endregion

    }
}
