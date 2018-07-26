using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

using Xunit;
using Microsoft.Extensions.Logging;

using app.tests.model;
using app.common.serialization;
using app.model;


// dotnet test --filter FullyQualifiedName~AppWireSerializerTest
namespace app.tests.email
{
    public class AppWireSerializerTest : AppUnitTest
    {
        
        public AppWireSerializerTest()
        {

        }

        [Fact]
        public void AppWireSerializerTest001_SerializeCustomer_ExpectNoExceptions()
        {
            var sz = new AppWireSerializer<Customer>();
            
            Customer entity = new Customer("CustomerMasterRepositoryTest001_cname", "1-800-start");
            this.testLogger.LogDebug($"*** Original:\n{entity.ToJson()}");

            sz.Serialize(entity);

            sz.Reset();
            var reconstructed = sz.Deserialize();
            this.testLogger.LogDebug($"*** Deserialized:\n{reconstructed.ToJson()}");

            // Assert.Equal(entity, reconstructed);

        }


        [Fact]
        public void AppWireSerializerTest002_SerializeCustomer_ExpectNoExceptions()
        {
            var sz = new AppWireSerializer<Customer>();
            
            Customer entity = new Customer("CustomerMasterRepositoryTest001_cname", "1-800-start");
            this.testLogger.LogDebug($"*** Original:\n{entity.ToJson()}");

            sz.Serialize(entity);
            var serialized = sz.GetSerializedData();

            sz.Reset();
            var reconstructed = sz.Deserialize(serialized);
            this.testLogger.LogDebug($"*** Deserialized:\n{reconstructed.ToJson()}");

            // Assert.Equal(entity, reconstructed);

        }


        [Fact]
        public void AppWireSerializerTest003_SerializeCustomerAppEvent_ExpectNoExceptions()
        {
            var sz = new AppWireSerializer<AppEventArgs<Customer>>();
            
            Customer entity = new Customer("CustomerMasterRepositoryTest001_cname", "1-800-start");
            
            AppEventArgs<Customer> evt = new AppEventArgs<Customer>(){
                beforeChange = new Customer(),
                afterChange = entity,
                appEventType = AppEventType.Insert
            };

            this.testLogger.LogDebug($"*** Original:\n{entity.ToJson()}");

            sz.Serialize(evt);
            var serialized = sz.GetSerializedData();

            sz.Reset();
            var reconstructed = sz.Deserialize(serialized);
            this.testLogger.LogDebug($"*** Deserialized:\n{reconstructed.ToJson()}");

            Assert.Equal(evt.id, reconstructed.id);

        }

    }
}