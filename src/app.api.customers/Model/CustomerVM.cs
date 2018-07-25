using System;
using System.ComponentModel.DataAnnotations;  
using System.ComponentModel.DataAnnotations.Schema;

using app.model;
using app.data;

namespace  app.api.customers.Model
{
    public class CustomerVM
    {
        public string entityid { get; set;} = "";

        [Required]
        public string name { get; set; }

        [Required]
        public string phone { get; set; }

        [Required]
        public string email { get; set; }

        public DateTime createdDate { get; set; } = DateTime.UtcNow;
    }
}