using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetSentinel.Api.Models
{   
    public class OllamaResponse
    {
        
        public int Id { get; set; }
        public string Model { get; set; } = string.Empty;

    }
}