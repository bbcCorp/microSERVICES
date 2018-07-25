using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

using app.model;

namespace app.data
{
    public interface IDbEntity<T> : IEntity<T>
    {
        // Unique ID for object
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        string entityid { get; set; }           
    } 
}    