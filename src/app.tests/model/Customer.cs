using System;
using System.ComponentModel.DataAnnotations;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

using app.data;
using app.model;

namespace app.tests.model
{
    public class Customer  : IDbEntity<Customer>, IAppSerializer<Customer>
    {
        [Key]        
        [JsonProperty]
        public long id  { get; set;} = -1;

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty]
        public string entityid { get; set;}


        [Required]
        [JsonProperty]
        public string name { get; set; }

        [Required]
        [JsonProperty]
        public string phone { get; set; }


        [Required]
        [JsonProperty]
        public DateTime createdDate { get; set; } = DateTime.UtcNow;

        [Required]
        [JsonProperty]
        public DateTime updatedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [JsonProperty]
        public bool deleted { get; set; } = false;

        public Customer(){
            this.entityid = ObjectId.GenerateNewId().ToString();

            this.name = "";
            this.phone = "";            
        }
        public Customer(string name, string phone)
        {
            this.entityid = ObjectId.GenerateNewId().ToString();

            this.name = name;
            this.phone = phone;
        }

        [JsonConstructor]
        public Customer(string entityid, long id, DateTime updatedDate, bool deleted, string name, string phone)
        {
            // Generic interface properties
            this.entityid = entityid;
            this.id = id;
            this.updatedDate = updatedDate;
            this.deleted = deleted;

            // Specific properties
            this.name = name;
            this.phone = phone;            
        }
        
        public Customer(Customer c)
        {
            // Generic interface properties
            this.entityid = c.entityid;
            this.id = c.id;
            this.updatedDate = c.updatedDate;
            this.deleted = c.deleted;

            // Specific properties
            this.name = c.name;
            this.phone = c.phone;
        }

        public Customer Clone()
        {
            return new Customer(this);
        }

        public string ToJson()
        {
            var objson = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return objson;
        }
        public static Customer GetFromJson(string serializedJSON)
        {
            Customer obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Customer>(serializedJSON);
            return obj;
        }
        Customer IAppSerializer<Customer>.FromJson(string serializedJSON)
        {
            return Customer.GetFromJson(serializedJSON);
        }
    }    
}