using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;

using app.model;
using app.tests.model;

using app.data;
using app.data.elastic;

namespace app.tests.mongo
{
    // dotnet test --filter FullyQualifiedName~ElasticRepositoryTest
    public class ElasticRepositoryTest : AppUnitTest
    {       
        private readonly ElasticRepository<Customer> _repo;

        public ElasticRepositoryTest()
        {
            this._repo = new ElasticRepository<Customer>(loggerFactory, 
                this.Configuration["ElasticService:ServerUrl"], null, 
                "Customer", 
                this.Configuration["ElasticService:AppIndex"]);
        }


        [Fact]
        public void ElasticRepositoryTest001_SingleCRUD_ExpectNoException()
        {
            
            this.testLogger.LogDebug($"Running test: ElasticRepositoryTest001_SingleCRUD_ExpectNoException");
            
            // Test Single Inserts
            var entity = new Customer { id = 1, name = "BBC", phone = "1-800-PHONE" };

            this._repo.Add(entity);
            this.testLogger.LogDebug("Created entry for customer");

            Assert.True(this._repo.Exists(1));

            // Retrieve by ID
            var readEntity = this._repo.GetById(1);
            this.testLogger.LogDebug($"Retrieved cust with id: 1");
            Assert.Equal(entity.ToJson(), readEntity.ToJson());

            // Test Updates
            entity.phone = "1-800-UPDATEDPHONE";
            this._repo.Update(entity);
            readEntity = this._repo.GetById(1);
            Assert.Equal("1-800-UPDATEDPHONE", readEntity.phone);

            // Test Single ID Deletion
            this.testLogger.LogDebug($"Deleting customer record");
            this._repo.Delete(1);

            // Expect exception
            Assert.False(this._repo.Exists(1));

        }

        [Fact]
        public void ElasticRepositoryTest002_BulkCRUD_ExpectNoException()
        {
            
            this.testLogger.LogDebug($"Running test: ElasticRepositoryTest002_BulkCRUD_ExpectNoException");

            // Test Bulk Inserts
            long[] entityIdLst = new long[100];
            List<Customer> entities = new List<Customer>();
            for (int i = 2; i < 102; i++)
            {
                entityIdLst[i - 2] = i;
                entities.Add(new Customer { id = i, name = $"BBC{i}", phone = $"{i}-800-PHONE" });
            }
            this._repo.Add(entities);
            this.testLogger.LogDebug("Created entries for customer");

            try
            {
                Assert.True(this._repo.Exists(50));

                // Retrieve by ID
                var readEntityLst = this._repo.GetById(entityIdLst);
                this.testLogger.LogDebug($"Retrieved {readEntityLst.Count} customers");
                Assert.Equal(100, readEntityLst.Count);

                // Test Bulk Updates
                entities.ForEach(e => e.phone = $"{e.id}-800-UPDATEDPHONE");
                this._repo.Update(entities);

                var readEntity = this._repo.GetById(50);
                Assert.Equal($"{readEntity.id}-800-UPDATEDPHONE", readEntity.phone);                
            }
            finally
            {
                this.testLogger.LogDebug($"Deleting {entityIdLst.Length} customer records");
                this._repo.Delete(entityIdLst);

                Assert.False(this._repo.Exists(50));

            }


        }


    }
}