
using System;
using System.ComponentModel.DataAnnotations;

namespace app.model
{
    public interface IEntity<T> 
    {
        [Key] 
        // Used to store sequence of object creation
        long id { get; set; }   

        [Required]
        DateTime updatedDate {get;set;}

        [Required]
        bool deleted { get; set;}   

        T Clone();   
  
    }
  
}