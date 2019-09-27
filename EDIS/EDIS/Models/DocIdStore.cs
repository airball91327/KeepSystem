using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Models
{
    public class DocIdStore
    {
        [Key, Column(Order = 1)]
        public string DocType { get; set; }
        [Key, Column(Order = 2)]
        public string DocId { get; set; }
    }
}
