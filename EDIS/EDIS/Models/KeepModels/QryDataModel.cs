using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS.Models.KeepModels
{
    public class QryKeepListData
    {
        public string KqtyDOCID { get; set; }
        public string KqtyASSETNO { get; set; }
        public string KqtyACCDPT { get; set; }
        public string KqtyASSETNAME { get; set; }
        public string KqtyFLOWTYPE { get; set; }
        public string KqtyDPTID { get; set; }
        public string KqtyKeepResult { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public string KqtyApplyDateFrom { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public string KqtyApplyDateTo { get; set; }
        public string KqtyDateType { get; set; }
        public string KqtyIsCharged { get; set; }
        public bool KqtySearchAllDoc { get; set; }
        public string KqtyEngCode { get; set; }
        public string KqtyTicketNo { get; set; }
        public string KqtyVendor { get; set; }
        public string KqtyOrderType { get; set; }
    }
}
