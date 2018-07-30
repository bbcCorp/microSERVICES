using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;

using app.model;
using app.data;
using app.data.mongo;
using app.tests.model;

namespace app.tests.mongo
{
    public class CustomerMasterRepositoryTest : AppUnitTest
    {       
        MongoRepository<Customer> repo = null;

        public CustomerMasterRepositoryTest()
        {
            var connection = this.Configuration["ConnectionStrings:CustomerDbConnection"];
            this.repo = new MongoRepository<Customer>(this.loggerFactory);
            this.repo.Setup(connection, "test_app_repo", "customermaster");
        }

        [Fact]
        public async Task CustomerMasterRepositoryTest001_CreateFindDeleteAsync_ExpectNoExceptions()
        {
            // Test cases for async API

            await repo.DeleteAllAsync();
            Assert.Equal(0, repo.Count()); 

            // Add an entity
            Customer entity = new Customer("CustomerMasterRepositoryTest001_cname", "1-800-start");                               
            await repo.AddAsync(entity);  
            this.testLogger.LogDebug( $"New entity: {entity.ToJson()}");

            // Count should increase by 1
            Assert.Equal(1, await repo.CountAsync());       

            // Test get by id
            var fetch = await repo.GetByEntityIdAsync(entity.entityid);
            Assert.NotNull(fetch);
            // Assert.Equal(fetch,entity);

            // Test search API
            var searchresult = await repo.GetAsync( e => e.phone=="1-800-start" );
            Assert.Equal(1, searchresult.Count); 

            // Test Update API
            entity.phone = "1-800-updated";
            await repo.UpdateAsync(entity);
            Assert.Equal(1, (await repo.GetAsync( e => e.phone=="1-800-updated" )).Count); 

            await repo.DeleteAsync(entity.entityid);
            await Assert.ThrowsAsync<Exception>( async () => fetch = await repo.GetByEntityIdAsync(entity.entityid));
          
        }        

        [Fact]
        public void CustomerMasterRepositoryTest002_CreateFindDeleteSync_ExpectNoExceptions()
        {
            if( repo.Count() > 0){
                repo.DeleteAll();
            }

            Customer entity = new Customer("CustomerMasterRepositoryTest002_cname", "1-800-start");

            repo.Add(entity);  

            long newCount = repo.Count();   
            Assert.Equal(1, newCount);       

            var fetch = repo.GetByEntityId(entity.entityid);
            Assert.NotNull(fetch);
            // Assert.Equal(fetch,entity);

            // Test search API
            var searchresult = repo.Get( e => e.phone=="1-800-start" );
            Assert.Equal(1, searchresult.Count); 

            // Test Update API
            entity.phone="1-800-updated";
            repo.Update(entity);
            Assert.Equal(1, (repo.Get( e => e.phone=="1-800-updated")).Count); 

            repo.Delete(entity.entityid);

            Assert.Throws<Exception>(() => fetch = repo.GetByEntityId(entity.entityid));
          
        }

        [Fact]
        public async Task CustomerMasterRepositoryTest003_TestBulkInsertUpdateSearchDeleteAsync_ExpectNoExceptions()
        {
            await repo.DeleteAllAsync();
            Assert.Equal(0, repo.Count()); 

            var testCount = 1000;
            List<Customer> entities = new List<Customer>();
            for(int i=0;i<testCount;i++){
                entities.Add(new Customer($"BulkItemTest003_cname_{i+1}", $"cphone_{i+1}"));
            }

            var opTimer = Stopwatch.StartNew();
            await repo.AddAsync(entities);        
            opTimer.Stop();
            this.testLogger.LogDebug($"BulkInsert :: Total execution time: {opTimer.Elapsed.TotalSeconds}");   

            Assert.Equal(testCount, repo.Count()); 

            // Test search API
            opTimer = Stopwatch.StartNew();
            var searchresult = await repo.GetAsync( e => e.name.StartsWith("BulkItemTest003_") );
            opTimer.Stop();
            Assert.Equal(testCount, searchresult.Count); 
            this.testLogger.LogDebug($"GetAsync :: Total execution time: {opTimer.Elapsed.TotalSeconds}");

            // Test bulk update
            entities.ForEach(i=> i.name = i.name.Replace("BulkItemTest003_", "BulkItemTestUpdate003_"));
            opTimer = Stopwatch.StartNew();
            await repo.UpdateAsync(entities);       
            opTimer.Stop();
            this.testLogger.LogDebug($"BulkInsert :: Total execution time: {opTimer.Elapsed.TotalSeconds}");  

            searchresult = await repo.GetAsync( entity => entity.name.StartsWith("BulkItemTestUpdate003_") );
            Assert.Equal(testCount, searchresult.Count); 

            opTimer = Stopwatch.StartNew();
            await repo.DeleteAllAsync();
            opTimer.Stop();
            this.testLogger.LogDebug($"DeleteAllAsync :: Total execution time: {opTimer.Elapsed.TotalSeconds}"); 

            Assert.Equal(0, repo.Count()); 

        }

        
        private Func<AppEventArgs<Customer>, Task> __getEventHandler(string description)
        {
            Func<AppEventArgs<Customer>, Task> handler = (earg) =>
            {
                return Task.Run(() =>
                {
                    StringBuilder msg = new StringBuilder();

                    msg.Append($"\n - EventType:{earg.appEventType} :: {description} -");
                    if(earg.beforeChange != null){
                        msg.Append($"- Before: {earg.beforeChange.ToJson()}" );
                    }

                    if(earg.afterChange != null){
                        msg.Append($"- After: {earg.afterChange.ToJson()}" );
                    }

                    this.testLogger.LogDebug(msg.ToString());
                });

            };
            return handler;
        }


        [Fact]
        public void CustomerMasterRepositoryTest005_TestEventFiring_ExpectNoExceptions()
        {
            repo.DeleteAll();

            var anyevt = repo.Subscribe(AppEventType.Any, __getEventHandler("H1: Handling ANY event"));

            var evt1 = repo.Subscribe(AppEventType.Insert, __getEventHandler("H1: Handling INSERT event"));
            var evt4 = repo.Subscribe(AppEventType.Insert, __getEventHandler("H2: Handling INSERT event"));

            var evt2 = repo.Subscribe(AppEventType.Update, __getEventHandler("H1: Handling UPDATE event"));
            var evt5 = repo.Subscribe(AppEventType.Update, __getEventHandler("H2: Handling UPDATE event"));

            var evt3 = repo.Subscribe(AppEventType.Delete, __getEventHandler("H1: Handling DELETE event"));
            var evt6 = repo.Subscribe(AppEventType.Delete, __getEventHandler("H2: Handling DELETE event"));

            this.testLogger.LogDebug("\n\n *** Adding entity ***");
            var entity = new Customer("CustomerMasterRepositoryTest005_Customer001", "1-800-PHONE");
            repo.Add(entity);
            this.testLogger.LogDebug(entity.ToJson());


            this.testLogger.LogDebug("\n\n *** Updating entity ***");
            entity.phone = "1-800-UPDATED";
            repo.Update(entity);

            this.testLogger.LogDebug("\n\n *** Deleting entity ***");
            repo.Delete(entity.entityid);

            repo.UnSubscribe(evt4);
            repo.UnSubscribe(evt5);
            repo.UnSubscribe(evt6);
            repo.UnSubscribe(anyevt);
            this.testLogger.LogDebug(" ============================= H2 unsubscribed ===========================");

            this.testLogger.LogDebug("\n\n *** Adding entity ***");
            entity = new Customer("CustomerMasterRepositoryTest005_Customer002", "1-800-PHONE");
            repo.Add(entity);

            this.testLogger.LogDebug("\n\n *** Updating entity ***");
            entity.phone = "1-800-UPDATED";
            repo.Update(entity);

            this.testLogger.LogDebug("\n\n *** Deleting entity ***");
            repo.Delete(entity.entityid);

            repo.UnSubscribe(evt3);
            repo.UnSubscribe(evt2);
            repo.UnSubscribe(evt1);

        }

    }
}