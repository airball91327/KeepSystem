using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EDIS.Models.RepairModels
{
    public class Ticket_seq_tmpModel
    {
        [Key]
        public string YYYMM { get; set; }
        public string TICKET_SEQ { get; set; }
    }
}
